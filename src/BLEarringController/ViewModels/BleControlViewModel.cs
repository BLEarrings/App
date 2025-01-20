using System.Diagnostics;
using System.Text;
using BLEarringController.Ble;
using BLEarringController.Ble.Services;
using BLEarringController.Views;
using Plugin.BLE.Abstractions.Contracts;

namespace BLEarringController.ViewModels
{
    public sealed class BleControlViewModel : ViewModelBase
    {
        #region Fields

        #region Private

        /// <summary>
        /// The current <see cref="IAdapter"/> used by the current
        /// <see cref="_bleImplementation"/>.
        /// </summary>
        private readonly IAdapter _bleAdapter;

        /// <summary>
        /// Backing variable for <see cref="BlueSliderValue"/>.
        /// </summary>
        private int _blueSliderValue;

        /// <summary>
        /// backing variable for <see cref="CardAspectRatio"/>.
        /// </summary>
        private double _cardAspectRatio;

        /// <summary>
        /// Backing variable for <see cref="GreenSliderValue"/>.
        /// </summary>
        private int _greenSliderValue;

        /// <summary>
        /// Backing variable for <see cref="RedSliderValue"/>.
        /// </summary>
        private int _redSliderValue;

        /// <summary>
        /// Backing variable for <see cref="SelectedBleDevice"/>.
        /// </summary>
        private IDevice? _selectedBleDevice;

        #endregion

        #endregion

        #region Construction

        public BleControlViewModel()
        {
            // Convenient variables to access the current IBluetoothLE and IAdapter objects.
            _bleImplementation = CrossBluetoothLE.Current;
            _bleAdapter = _bleImplementation.Adapter;

            // Create commands to invoke methods when UI buttons are clicked.
            SelectDeviceCommand = new Command(SelectDeviceCommandTask);
            SendColourCommand = new Command(SendColourCommandTask);
        }

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// Text to display on the device information card when a property of the BLE device is
        /// <c>null</c>.
        /// </summary>
        public string BleDeviceNullFallback => "Null";

        /// <summary>
        /// The value selected on the blue slider.
        /// </summary>
        public int BlueSliderValue
        {
            get => _blueSliderValue;
            set
            {
                if (_blueSliderValue != value)
                {
                    _blueSliderValue = value;
                    NotifyPropertyChanged();

                    // Raise PropertyChanged notification for derived properties.
                    NotifyPropertyChanged(nameof(BlueThumbColor));
                    NotifyPropertyChanged(nameof(SelectedColor));
                }
            }
        }

        /// <summary>
        /// The <see cref="Color"/> to display on the thumb of the blue slider. This represents the
        /// intensity of the current <see cref="BlueSliderValue"/>.
        /// </summary>
        public Color BlueThumbColor => Color.FromRgb(0, 0, _blueSliderValue);

        // TODO: Temporary aspect ratio slider value for testing.
        public double CardAspectRatio
        {
            get => _cardAspectRatio;
            set
            {
                if (Math.Abs(_cardAspectRatio - value) > 0.01)
                {
                    _cardAspectRatio = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The value selected on the green slider.
        /// </summary>
        public int GreenSliderValue
        {
            get => _greenSliderValue;
            set
            {
                if (_greenSliderValue != value)
                {
                    _greenSliderValue = value;
                    NotifyPropertyChanged();

                    // Raise PropertyChanged notification for derived properties.
                    NotifyPropertyChanged(nameof(GreenThumbColor));
                    NotifyPropertyChanged(nameof(SelectedColor));
                }
            }
        }

        /// <summary>
        /// The <see cref="Color"/> to display on the thumb of the green slider. This represents
        /// the intensity of the current <see cref="GreenSliderValue"/>.
        /// </summary>
        public Color GreenThumbColor => Color.FromRgb(0, _greenSliderValue, 0);

        /// <summary>
        /// Text to display in the "Name" field when a device is not selected.
        /// </summary>
        public string NoBleDeviceName => "No BLE device selected.";

        /// <summary>
        /// Text to display over the top of the card before a device is selected.
        /// </summary>
        public string NoDeviceSelectedCardText => "No device selected. Tap this card to select a device.";

        /// <summary>
        /// The value selected on the red slider.
        /// </summary>
        public int RedSliderValue
        {
            get => _redSliderValue;
            set
            {
                if (_redSliderValue != value)
                {
                    _redSliderValue = value;
                    NotifyPropertyChanged();

                    // Raise PropertyChanged notification for derived properties.
                    NotifyPropertyChanged(nameof(RedThumbColor));
                    NotifyPropertyChanged(nameof(SelectedColor));
                }
            }
        }

        /// <summary>
        /// The <see cref="Color"/> to display on the thumb of the red slider. This represents the
        /// intensity of the current <see cref="BlueSliderValue"/>.
        /// </summary>
        public Color RedThumbColor => Color.FromRgb(_redSliderValue, 0, 0);

        /// <summary>
        /// The <see cref="Command"/> that will wrap the <see cref="SelectDeviceCommandTask"/>.
        /// </summary>
        public Command SelectDeviceCommand { get; }

        /// <summary>
        /// The currently selected <see cref="IDevice"/>.
        /// </summary>
        /// <remarks>
        /// This will be <c>null</c> before a device is selected.
        /// </remarks>
        public IDevice? SelectedBleDevice
        {
            get => _selectedBleDevice;
            set
            {
                // To check if a new value is the same device, compare the MAC addresses.
                if (_selectedBleDevice?.Id != value?.Id)
                {
                    _selectedBleDevice = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The <see cref="Color"/> represented by the current <see cref="RedSliderValue"/>,
        /// <see cref="GreenSliderValue"/>, and <see cref="BlueSliderValue"/> selections.
        /// </summary>
        public Color SelectedColor => Color.FromRgb(_redSliderValue, _greenSliderValue, _blueSliderValue);

        /// <summary>
        /// Text to display on the button that sends the <see cref="SelectedColor"/> to the
        /// <see cref="SelectedBleDevice"/>.
        /// </summary>
        public string SendColourButtonText => "Send colour to device";

        /// <summary>
        /// The <see cref="Command"/> that will wrap the <see cref="SendColourCommandTask"/>.
        /// </summary>
        public Command SendColourCommand { get; }

        #endregion

        #endregion

        #region Methods

        #region Private

        /// <summary>
        /// Send the <see cref="SelectedColor"/> to the <see cref="SelectedBleDevice"/>.
        /// </summary>
        private async void SendColourCommandTask()
        {
            // Wrap the entire method in a try-catch to prevent exceptions crashing the process,
            // since this is an async void.
            try
            {
                // If no device is selected, colour cannot be sent so just return.
                if (SelectedBleDevice == null)
                {
                    // TODO: display popup to user.
                    return;
                }
                // Ensure device is connected before colour can be sent.
                await _bleAdapter.ConnectToDeviceAsync(SelectedBleDevice);
                if (SelectedBleDevice.State != DeviceState.Connected)
                {
                    // TODO: display popup to user.
                    return;
                }

                // Get the Nordic UART Service and RX characteristic from the device to send the
                // colour to.
                if (await SelectedBleDevice.GetServiceAsync(NordicUart.ServiceGuid) is { } uartService
                    && await uartService.GetCharacteristicAsync(NordicUart.RxCharacteristicGuid) is { CanWrite: true } rxCharacteristic)
                {
                    // TODO: Notify user that sending the colour failed.
                }
            }
            catch
            {
                // Ignore.
                // TODO: Log exception to user.
            }
        }

        #endregion

        #region Public

        /// <inheritdoc />
        public override void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Apply any supported NavigationQueryKeys after navigation.
            if (query.TryGetValue(NavigationQueryKeys.SelectedBleDeviceKey, out var selection)
                && selection is IDevice selectedBleDevice)
            {
                // If the query contains an IDevice, apply it to the ViewModel.
                SelectedBleDevice = selectedBleDevice;
            }
        }

        #endregion

        #region Private Static

        /// <summary>
        /// Opens the <see cref="KnownModals.BleScanView"/> to allow the user to select a
        /// <see cref="IDevice"/> from a BLE scan.
        /// </summary>
        private static async void SelectDeviceCommandTask()
        {
            // Wrap the entire method in a try-catch to prevent exceptions crashing the process,
            // since this is an async void.
            try
            {
                // Open the BleScanView modal to allow the user to select a BLE device.
                await Shell.Current.GoToAsync(KnownModals.BleScanView);
            }
            catch (Exception ex)
            {
                // TODO: Alert user!
                // In Debug, always break here as this is unexpected!
                Debug.Assert(false, ex.Message);
            }
        }

        #endregion

        #endregion
    }
}
