using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using Plugin.BLE;
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
        /// The number of milliseconds between updates of <see cref="ScanProgress"/>.
        /// </summary>
        private const int ScanProgressInterval = 5;

        /// <summary>
        /// The timeout in milliseconds to use for the BLE scan.
        /// </summary>
        private const int ScanTimeout = 500;

        /// <summary>
        /// The amount that the <see cref="ScanProgress"/> should increase by every
        /// <see cref="ScanProgressInterval"/>, based on the <see cref="ScanTimeout"/>.
        /// </summary>
        private static readonly double ScanProgressTick =
            1d /(ScanTimeout / ScanProgressInterval);

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

            // Create the command that will run whenever the "Scan" button is clicked.
            ScanCommand = new Command(ScanCommandTask);

            // Convenient variables to access the current IBluetoothLE and IAdapter objects.
            _bleImplementation = CrossBluetoothLE.Current;
            _bleAdapter = _bleImplementation.Adapter;

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
        public double ScanProgress
        {
            get => _scanProgress;
            set
            {
                if (_scanProgress != value)
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

        #region Public

        /// <summary>
        /// The method wrapped by the <see cref="ScanCommand"/> which starts a BLE scan.
        /// </summary>
        public async void ScanCommandTask()
        {
            if (!await _scanSemaphore.WaitAsync(ScanTimeout))
            {
                // Wait for the entire duration of a standard scan to enter the semaphore. If this
                // fails, simply return as the scan cannot begin.
                return;
            }

            // The entire method is wrapped in a try-catch since it is an async void, which can
            // cause the process to crash if an exception is raised. The BLE scan is fairly likely
            // to throw an exception, so ensure it gets caught.
            try
            {
                // The _scanSemaphore.CurrentCount will have been altered, so ensure the
                // ScanInProgress property is updated on the UI.
                NotifyPropertyChanged(nameof(ScanInProgress));

                // Clear all previous results from the collection.
                FoundDevices.Clear();

                // Reset scan progress back to 0 at the start of the scan.
                ScanProgress = 0;

                // Start incrememnting the ScanProgress.
                _scanProgressIntervalTimer.Start();

                // Subscribe to DeviceDiscovered events, which will add the BLE devices to the
                // FoundDevices collection as they are discovered.
                _bleAdapter.DeviceDiscovered += _bleAdapter_DeviceDiscovered;

                // Perform a BLE scan, cancelling using the ScanTimeoutToken after the ScanTimeout.
                await _bleAdapter.StartScanningForDevicesAsync(ScanTimeoutToken);
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
                ScanHasRun |= true;

                // Exit the semaphore, and update the ScanInProgress property since the semaphore's
                // CurrentCount has changed again.
                _scanSemaphore.Release();
                NotifyPropertyChanged(nameof(ScanInProgress));
            }
        }

        #endregion

        #endregion
    }
}
