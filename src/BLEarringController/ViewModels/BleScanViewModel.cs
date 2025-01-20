using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using BLEarringController.Services;
using BLEarringController.Views;
using CommunityToolkit.Mvvm.Input;
using Plugin.BLE.Abstractions.Contracts;
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
        /// The singleton service used to interact with BLE devices.
        /// </summary>
        private readonly IBleManager _bleManager;

        /// <summary>
        /// Service used to display notifications to the user.
        /// </summary>
        private readonly INotificationManager _notificationManager;

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

        public BleScanViewModel(IBleManager bleManager, INotificationManager notificationManager)
        {
            // Store injected dependencies.
            _bleManager = bleManager;
            _notificationManager = notificationManager;

            // Initialise the FoundDevices collection to be empty before a scan has run.
            FoundDevices = [];

            // Create commands to invoke methods when UI buttons are clicked.
            ScanCommand = new AsyncRelayCommand(ScanCommandTask);
            DeviceSelectedCommand = new AsyncRelayCommand<IDevice>(DeviceSelectedCommandTask);

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
        /// The <see cref="IRelayCommand"/> that will wrap the
        /// <see cref="DeviceSelectedCommandTask"/>.
        /// </summary>
        public IRelayCommand DeviceSelectedCommand { get; }

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
        /// The <see cref="IRelayCommand"/> that will wrap the <see cref="ScanCommandTask"/>.
        /// </summary>
        public IRelayCommand ScanCommand { get; }

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

        #endregion

        #region Methods

        #region Event Handlers

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

        /// <summary>
        /// The method wrapped by <see cref="DeviceSelectedCommand"/> which returns from the
        /// scan view with the selected device as a navigation parameter.
        /// </summary>
        /// <param name="selectedDevice">The <see cref="IDevice"/> selected by the user.</param>
        /// <returns></returns>
        private async Task DeviceSelectedCommandTask(IDevice? selectedDevice)
        {
            if (selectedDevice == null)
            {
                // The selected device must be non-null for navigation to occur through selection.
                return;
            }
            // TODO: Currently bonding is not required for the BLEarrings.
            //if (!await _bleManager.Bond(selectedDevice))
            //{
            //    // TODO: Display alert indicating bonding failure.
            //    return;
            //}
            if (!await _bleManager.Connect(selectedDevice))
            {
                await _notificationManager.DisplayAlert(
                    "Failed to connect to device",
                    "Could not connect to the selected device. Please try again.");
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

        /// <summary>
        /// The method wrapped by the <see cref="ScanCommand"/> which starts a BLE scan.
        /// </summary>
        private async Task ScanCommandTask()
        {
            // Wait for the entire duration of a standard scan to enter the semaphore. If this
            // fails, simply return as the scan cannot begin.
            if (!await _scanSemaphore.WaitAsync(ScanTimeout))
            {
                await _notificationManager.DisplayAlert(
                    "Failed to start BLE scan.",
                    "Timed out waiting for the previous BLE scan to finish.");

                return;
            }

            // A try-finally is used to ensure the semaphore is always released.
            try
            {
                if (!await _bleManager.RequestPermission("Bluetooth permission is required to perform a BLE scan."))
                {
                    return;
                }

                // The _scanSemaphore.CurrentCount will have been altered, so ensure the
                // ScanInProgress property is updated on the UI.
                NotifyPropertyChanged(nameof(ScanInProgress));

                // Clear all previous results from the collection.
                FoundDevices.Clear();

                // Reset scan progress back to 0 at the start of the scan.
                ScanProgress = 0;

                // Start incrementing the ScanProgress as close to starting the scan as possible,
                // to try and ensure it accurately represents the scan progress.
                _scanProgressIntervalTimer.Start();

                // TODO: This will not update this list "live", but would feel more responsive if it did...
                var foundDevices = await _bleManager.Scan(ScanTimeout);

                foreach (var device in foundDevices)
                {
                    FoundDevices.Add(device);
                }
            }
            catch (Exception ex)
            {
                // TODO: Logging?
                // This should never happen...
                Debug.Assert(false, ex.Message);
            }
            finally
            {
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

        #endregion
    }
}
