using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using BLEarringController.Ble.Services;
using BLEarringController.Views;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;
using Timer = System.Timers.Timer;

namespace BLEarringController.ViewModels
{
    public sealed class BleScanViewModel : ViewModelBase
    {
        #region Constants

        #region Private

        /// <summary>
        /// The tolerance to use when comparing <see cref="ScanProgress"/> values to check whether
        /// the value has changed.
        /// </summary>
        private const double ProgressComparisonTolerance = 0.1;

        /// <summary>
        /// The number of milliseconds between updates of <see cref="ScanProgress"/>.
        /// </summary>
        private const int ScanProgressInterval = 5;

        /// <summary>
        /// The amount that the <see cref="ScanProgress"/> should increase by every
        /// <see cref="ScanProgressInterval"/>, based on the <see cref="ScanTimeout"/>.
        /// </summary>
        private const double ScanProgressTick = 1d / ((double)ScanTimeout / ScanProgressInterval);

        /// <summary>
        /// The timeout in milliseconds to use for the BLE scan.
        /// </summary>
        private const int ScanTimeout = 500;

        #endregion

        #endregion

        #region Fields

        #region Private

        /// <summary>
        /// The current <see cref="IAdapter"/> used by the current
        /// <see cref="_bleImplementation"/>.
        /// </summary>
        private readonly IAdapter _bleAdapter;

        /// <summary>
        /// The current <see cref="IBluetoothLE"/> implementation on the device.
        /// </summary>
        private readonly IBluetoothLE _bleImplementation;

        /// <summary>
        /// Backing variable for <see cref="ScanHasRun"/>.
        /// </summary>
        private bool _scanHasRun;

        /// <summary>
        /// Backing variable for <see cref="ScanProgress"/>.
        /// </summary>
        private double _scanProgress;

        /// <summary>
        /// A <see cref="Timer"/> which elapses every <see cref="ScanProgressInterval"/> to
        /// increment <see cref="ScanProgress"/> by <see cref="ScanProgressTick"/>.
        /// </summary>
        private readonly Timer _scanProgressIntervalTimer = new(ScanProgressInterval);

        /// <summary>
        /// A <see cref="SemaphoreSlim"/> to ensure that only one BLE scan can run at a time.
        /// </summary>
        private readonly SemaphoreSlim _scanSemaphore = new(1, 1);

        #endregion

        #endregion

        #region Construction

        public BleScanViewModel()
        {
            // Initialise the FoundDevices collection to be empty before a scan has run.
            FoundDevices = [];

            // Convenient variables to access the current IBluetoothLE and IAdapter objects.
            _bleImplementation = CrossBluetoothLE.Current;
            _bleAdapter = _bleImplementation.Adapter;

            // Create commands to invoke methods when UI buttons are clicked.
            ScanCommand = new Command(ScanCommandTask);
            DeviceSelectedCommand = new Command(DeviceSelectedCommandTask);

            // Hook up the event handler that will update the ScanProgress.
            _scanProgressIntervalTimer.Elapsed += _scanProgressIntervalTimer_Elapsed;
        }

        #endregion

        #region Finalize

        ~BleScanViewModel()
        {
            // Remove event handlers and ensure the _scanProgressIntervalTimer has stopped.
            _scanProgressIntervalTimer.Elapsed -= _scanProgressIntervalTimer_Elapsed;
            _scanProgressIntervalTimer.Stop();
        }

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// The text displayed on the empty <see cref="CollectionView"/> when the view first opens,
        /// before a scan has been performed.
        /// </summary>
        public string BeforeFirstScanText => "Start a scan to discover devices.";

        /// <summary>
        /// The <see cref="Command"/> that will wrap the <see cref="DeviceSelectedCommandTask"/>.
        /// </summary>
        public Command DeviceSelectedCommand { get; }

        /// <summary>
        /// A list of <see cref="IDevice"/>s discovered during the scan.
        /// </summary>
        public ObservableCollection<IDevice> FoundDevices { get; }

        /// <summary>
        /// The text displayed on the empty <see cref="CollectionView"/> after a scan has been run,
        /// and no devices have been discovered.
        /// </summary>
        public string NoDevicesFoundText => "No devices found. Try again.";

        /// <summary>
        /// The text to display on the button to start the <see cref="ScanCommandTask"/>.
        /// </summary>
        public string ScanButtonText => "Start Scan";

        /// <summary>
        /// The <see cref="Command"/> that will wrap the <see cref="ScanCommandTask"/>.
        /// </summary>
        public Command ScanCommand { get; }

        /// <summary>
        /// A <see cref="bool"/> representing whether a scan has been run yet or not.
        /// </summary>
        public bool ScanHasRun
        {
            get => _scanHasRun;
            set
            {
                if (_scanHasRun != value)
                {
                    _scanHasRun = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// A <see cref="bool"/> indicating whether a scan is currently in progress.
        /// </summary>
        /// <remarks>
        /// This property is derived from the <see cref="_scanSemaphore"/>, so any calls that enter
        /// the <see cref="_scanSemaphore"/> should invoke the <c>PropertyChanged</c> event for
        /// this property, both when entering and exiting the <see cref="SemaphoreSlim"/>.
        /// </remarks>
        public bool ScanInProgress => _scanSemaphore.CurrentCount == 0;

        /// <summary>
        /// The text displayed on the empty <see cref="CollectionView"/> while a scan is in
        /// progress.
        /// </summary>
        public string ScanningText => "Scanning for devices...";

        /// <summary>
        /// A <see cref="double"/> representing the total progress of the scan. 0 represents 0%
        /// completion, and 1 represents 100% completion.
        /// </summary>
        /// <remarks>
        /// This is only updated when the difference between the new value and old value is greater
        /// than <see cref="ProgressComparisonTolerance"/>.
        /// </remarks>
        public double ScanProgress
        {
            get => _scanProgress;
            set
            {
                if (Math.Abs(_scanProgress - value) > ProgressComparisonTolerance)
                {
                    _scanProgress = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The text displayed beneath the <see cref="Title"/> ov the view.
        /// </summary>
        public string Subtitle => "Scan for nearby BLE devices.";

        /// <summary>
        /// The title displayed at the top of the view.
        /// </summary>
        public string Title => "BLE Scan";

        #endregion

        #region Private Static

        /// <summary>
        /// A static <see cref="CancellationToken"/> generator, used to create a
        /// <see cref="CancellationToken"/> that elapses after <see cref="ScanTimeout"/>
        /// milliseconds. This is used to set the timeout for the BLE scan.
        /// </summary>
        private static CancellationToken ScanTimeoutToken => new CancellationTokenSource(ScanTimeout).Token;

        #endregion

        #endregion

        #region Methods

        #region Event Handlers

        /// <summary>
        /// Handler for <see cref="IDevice"/>s that are discovered during the BLE scan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _bleAdapter_DeviceDiscovered(object? sender, DeviceEventArgs e)
        {
            FoundDevices.Add(e.Device);
        }

        /// <summary>
        /// Handler for the <see cref="_scanProgressIntervalTimer"/> elapsing, indicating that the
        /// <see cref="ScanProgress"/> should be incremented by <see cref="ScanProgressTick"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _scanProgressIntervalTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            ScanProgress += ScanProgressTick;
        }

        #endregion

        #region Private

        // TODO: Bonding isn't currently required.
        ///// <summary>
        ///// Attempt to bond to an <see cref="IDevice"/>, and return a success <see cref="bool"/>.
        ///// </summary>
        ///// <param name="device">The <see cref="IDevice"/> to bond to.</param>
        ///// <returns>
        ///// <c>true</c> if the device was bonded successfully. <c>false</c> otherwise.
        ///// </returns>
        //private async Task<bool> BondToDevice(IDevice device)
        //{
        //    try
        //    {
        //        // Attempt to bond to the device.
        //        await _bleAdapter.BondAsync(device);
        //    }
        //    catch
        //    {
        //        // Catch any exception, as the returned bool will already indicate whether the
        //        // bonding operation was successful or not.
        //    }

        //    // Return true if the device is now "bonded".
        //    return device.BondState == DeviceBondState.Bonded;
        //}

        /// <summary>
        /// Attempt to connect to an <see cref="IDevice"/>, and return a success
        /// <see cref="bool"/>.
        /// </summary>
        /// <param name="device">The <see cref="IDevice"/> to connect to.</param>
        /// <returns>
        /// <c>true</c> if the device was connected successfully. <c>false</c> otherwise.
        /// </returns>
        private async Task<bool> ConnectToDevice(IDevice device)
        {
            try
            {
                // Attempt to connect to the device.
                await _bleAdapter.ConnectToDeviceAsync(device);
            }
            catch
            {
                // Catch any exception, as the returned bool will already indicate whether the
                // connection operation was successful or not.
            }

            // Return true if the device is now connected.
            return device.State == DeviceState.Connected;
        }

        private async void DeviceSelectedCommandTask(object? selectedItem)
        {
            // Try catch around the entire method to prevent an exception from crashing the
            // process, as this method is an async void.
            try
            {
                if (selectedItem is not IDevice selectedDevice)
                {
                    // Sanity check, ensure cast of object? to IDevice works.
                    return;
                }
                // TODO: Currently bonding is not required for the BLEarrings.
                //if (!await BondToDevice(selectedDevice))
                //{
                //    // TODO: Display alert indicating bonding failure.
                //    return;
                //}
                if (!await ConnectToDevice(selectedDevice))
                {
                    // TODO: Display alert indicating device selection failure.
                    return;
                }

                // A valid device has been selected and connected to. Create
                // ShellNavigationQueryParameters to return the IDevice to the view that requested
                // the device selection.
                var navigationParameter = new ShellNavigationQueryParameters
                {
                    { NavigationQueryKeys.SelectedBleDeviceKey, selectedDevice }
                };

                // Navigate back to the previous view, passing back the selected IDevice.
                await Shell.Current.GoToAsync("..", navigationParameter);
            }
            catch (Exception ex)
            {
                // TODO: Warn the user.
                // Always break here in debug, as this is unexpected...
                Debug.Assert(false, ex.Message);
            }
        }

        /// <summary>
        /// The method wrapped by the <see cref="ScanCommand"/> which starts a BLE scan.
        /// </summary>
        private async void ScanCommandTask()
        {
            // The entire method is wrapped in a try-catch since it is an async void, which can
            // cause the process to crash if an exception is raised. The BLE scan is fairly likely
            // to throw an exception, so ensure it gets caught.
            try
            {
                if (await Permissions.RequestAsync<Permissions.Bluetooth>() != PermissionStatus.Granted)
                {
                    // TODO: Warn user with popup? Disable view until permission accepted?
                    return;
                }
                if (!await _scanSemaphore.WaitAsync(ScanTimeout))
                {
                    // TODO: Warn user with popup?
                    // Wait for the entire duration of a standard scan to enter the semaphore. If
                    // this fails, simply return as the scan cannot begin.
                    return;
                }

                // The _scanSemaphore.CurrentCount will have been altered, so ensure the
                // ScanInProgress property is updated on the UI.
                NotifyPropertyChanged(nameof(ScanInProgress));

                // Clear all previous results from the collection.
                FoundDevices.Clear();

                // Reset scan progress back to 0 at the start of the scan.
                ScanProgress = 0;

                // Prepare the options to use to filter scan results.
                var scanFilterOptions = new ScanFilterOptions
                {
                    // Scan only for devices with the NordicUart service.
                    ServiceUuids = [NordicUart.ServiceGuid]
                };

                // Scan using the highest duty cycle as the scan timeout is short.
                _bleAdapter.ScanMode = ScanMode.LowLatency;

                // Subscribe to DeviceDiscovered events, which will add the BLE devices to the
                // FoundDevices collection as they are discovered.
                _bleAdapter.DeviceDiscovered += _bleAdapter_DeviceDiscovered;

                // Start incrementing the ScanProgress as close to starting the scan as possible,
                // to try and ensure it accurately represents the scan progress.
                _scanProgressIntervalTimer.Start();

                // Perform a BLE scan, cancelling using the ScanTimeoutToken after the ScanTimeout.
                await _bleAdapter.StartScanningForDevicesAsync(scanFilterOptions, ScanTimeoutToken);

                // Devices that are already connected won't show up in the scan. Add any devices
                // that are already connected, that have the Nordic UART Service in the
                // advertisement records.
                foreach (var connectedDevice in _bleAdapter.ConnectedDevices)
                {
                    if (connectedDevice.AdvertisementRecords.Any(IsNordicUartAdvertisementRecord))
                    {
                        FoundDevices.Add(connectedDevice);
                    }
                }
            }
            catch (DeviceDiscoverException ex)
            {
                // Exception raised whenever device discovery fails.
                // TODO: Add a message popup to warn the user when this occurs.
                Debug.Assert(false, ex.Message);
            }
            catch (Exception ex)
            {
                // Catch any other exceptions that may be thrown from the OS.
                // TODO: Add a message popup to warn the user when this occurs.
                Debug.Assert(false, ex.Message);
            }
            finally
            {
                // Always ensure the EventHandler is unsubscribed to ensure devices aren't
                // duplicated during the next scan.
                _bleAdapter.DeviceDiscovered -= _bleAdapter_DeviceDiscovered;

                // Always stop incrementing the ScanProgress, and ensure it is indicated on the UI
                // that the scan completed by forcing the progress bar to 100%.
                _scanProgressIntervalTimer.Stop();
                ScanProgress = 1;

                // After a scan has run, ensure ScanHasRun is set to true. This should never be set
                // beck to false.
                ScanHasRun = true;

                // Exit the semaphore, and update the ScanInProgress property since the semaphore's
                // CurrentCount has changed again.
                _scanSemaphore.Release();
                NotifyPropertyChanged(nameof(ScanInProgress));
            }
        }

        #endregion

        #region Private Static

        /// <summary>
        /// Checks an <see cref="AdvertisementRecord"/> to see if it advertises the
        /// <see cref="NordicUart.ServiceId"/>.
        /// </summary>
        /// <param name="record">
        /// The <see cref="AdvertisementRecord"/> to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="AdvertisementRecord"/> is for the Nordic Uart Service.
        /// <c>false</c> otherwise.
        /// </returns>
        private static bool IsNordicUartAdvertisementRecord(AdvertisementRecord record)
        {
            return record is { Type: AdvertisementRecordType.ServiceData128Bit }
                   && record.Data == NordicUart.ServiceGuid.ToByteArray();
        }

        #endregion

        #endregion
    }
}
