using System.Diagnostics;
using System.Text;
using BLEarringController.Ble;
using BLEarringController.Ble.Services;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;

namespace BLEarringController.Services
{
    /// <inheritdoc cref="IBleManager" />
    public class BleManager : IBleManager
    {
        #region Fields

        #region Private

        /// <summary>
        /// The current Bluetooth LE adapter, used to interact with BLE devices.
        /// </summary>
        private readonly IAdapter _bleAdapter;

        /// <summary>
        /// The current Bluetooth LE implementation, used to manage and control the BLE
        /// functionality on the device.
        /// </summary>
        private readonly IBluetoothLE _bleImplementation;

        /// <summary>
        /// A <see cref="SemaphoreSlim"/> used to ensure that all BLE operations are sequential,
        /// and that only a single operation can run at a time.
        /// </summary>
        private readonly SemaphoreSlim _bleSemaphore;

        /// <summary>
        /// A <see cref="List{T}"/> of <see cref="IDevice"/>s found during the
        /// <see cref="Scan()"/> and <see cref="Scan(int)"/> methods.
        /// </summary>
        /// <remarks>
        /// This is used to allow the <see cref="_bleAdapter_DeviceDiscovered"/> event handler to
        /// add to a list accessible during the <see cref="Scan()"/> methods.
        /// </remarks>
        private readonly List<IDevice> _foundDevices;

        /// <summary>
        /// Service to allow displaying alerts and prompts to the user.
        /// </summary>
        private readonly INotificationManager _notificationManager;

        #endregion

        #endregion

        #region Construction

        public BleManager(
            IBluetoothLE bleImplementation,
            IAdapter bleAdapter,
            INotificationManager notificationManager)
        {
            // Store injected dependencies.
            _bleImplementation = bleImplementation;
            _bleAdapter = bleAdapter;
            _notificationManager = notificationManager;

            // Create a SemaphoreSlim to ensure that a single BLE operation can run at a time. The
            // initialCount is set to 1 to allow a single BLE operation to run at a time as soon as
            // the BleManager is created, and the maxCount is also set to 1 to catch errors where a
            // method may release the semaphore too many times, or before entering the semaphore.
            _bleSemaphore = new SemaphoreSlim(1, 1);

            // Initialise an empty list for the found devices.
            _foundDevices = [];
        }

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
            // TODO: Is this a good place to check for the required characteristics and fetch
            // TODO: them so they can be used immediately?
            // Add device to the internal list to collect devices found during the scan.
            _foundDevices.Add(e.Device);
        }

        #endregion

        #region Public

        /// <inheritdoc />
        public async Task<bool> Bond(IDevice device)
        {
            if (!await CheckPermission())
            {
                // If the app does not have Bluetooth permission, it cannot connect to Bluetooth
                // devices.
                // TODO: Better return here?
                return false;
            }

            // Wait to enter the semaphore to ensure a single BLE operation runs at a time.
            await _bleSemaphore.WaitAsync();

            try
            {
                // Attempt to bond to the device.
                await _bleAdapter.BondAsync(device);
            }
            catch
            {
                // Catch any exception, as the returned bool will already indicate whether the
                // bonding operation was successful or not.
            }
            finally
            {
                // Always release the semaphore.
                _bleSemaphore.Release();
            }

            // Return true if the device is now "bonded".
            return device.BondState == DeviceBondState.Bonded;
        }

        /// <inheritdoc />
        public async Task<bool> CheckPermission()
        {
            // Check the current status of the Bluetooth permission.
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();

            // Return true if the permission has already been granted.
            return permissionStatus == PermissionStatus.Granted;
        }

        /// <inheritdoc />
        public async Task<bool> Connect(IDevice device)
        {
            if (!await CheckPermission())
            {
                // If the app does not have Bluetooth permission, it cannot connect to Bluetooth
                // devices.
                // TODO: Better return here?
                return false;
            }

            // Wait to enter the semaphore to ensure a single BLE operation runs at a time.
            await _bleSemaphore.WaitAsync();

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
            finally
            {
                // Always release the Semaphore.
                _bleSemaphore.Release();
            }

            // Return true if the device is now connected.
            return device.State == DeviceState.Connected;
        }

        /// <inheritdoc />
        public async Task<bool> RequestPermission(string message)
        {
            if (await CheckPermission())
            {
                // If the app already has Bluetooth permission, return quickly with no prompt.
                return true;
            }

            // This method doesn't need to enter the _bleSemaphore, as it does not interact with
            // any BLE devices or the adapter.

            // Show an alert to explain to the user why the permission is being requested. In this
            // case, the alert is purely informational so has no return.
            await _notificationManager.DisplayAlert(
                "Bluetooth Permission is Required",
                message);

            // After explaining the permission request, start the request from the user.
            var permissionStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();

            // Return true if the user granted the permission.
            return permissionStatus == PermissionStatus.Granted;
        }

        /// <inheritdoc />
        public Task<List<IDevice>> Scan()
        {
            // Run a scan with the default timeout setting.
            return Scan(IBleManager.DefaultScanTimeout);
        }

        // TODO: Better device return method to update UI live?
        /// <inheritdoc />
        public async Task<List<IDevice>> Scan(int timeout)
        {
            if (!await CheckPermission())
            {
                // If the Bluetooth permission has not been granted yet, return an empty list.
                // TODO: Make this return easier to distinguish? BleScanResult?
                // TODO: Class with a "ScanEndedReason?" Permission error / timeout etc.
                return [];
            }

            // Wait to enter the semaphore to ensure a single BLE operation runs at a time.
            await _bleSemaphore.WaitAsync();

            // Clear the list of devices if it was populated in a previous scan.
            if (_foundDevices.Count > 0)
            {
                _foundDevices.Clear();
            }

            try
            {
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

                var scanToken = new CancellationTokenSource(timeout).Token;

                // Perform a BLE scan, cancelling using the ScanTimeoutToken after the ScanTimeout.
                await _bleAdapter.StartScanningForDevicesAsync(scanFilterOptions, scanToken);

                // Devices that are already connected won't show up in the scan. Add any devices
                // that are already connected, that have the Nordic UART Service in the
                // advertisement records.
                foreach (var connectedDevice in _bleAdapter.ConnectedDevices)
                {
                    if (connectedDevice.AdvertisementRecords.Any(IsNordicUartAdvertisementRecord))
                    {
                        _foundDevices.Add(connectedDevice);
                    }
                }
            }
            catch (DeviceDiscoverException ex)
            {
                await _notificationManager.DisplayAlert("Exception occurred during scan", ex.Message);

                // Exception raised whenever device discovery fails.
                Debug.Assert(false, ex.Message);
            }
            catch (Exception ex)
            {
                await _notificationManager.DisplayAlert("Exception occurred during scan", ex.Message);

                // Catch any other exceptions that may be thrown from the OS.
                Debug.Assert(false, ex.Message);
            }
            finally
            {
                // Always ensure the EventHandler is unsubscribed to ensure devices aren't
                // duplicated during the next scan.
                _bleAdapter.DeviceDiscovered -= _bleAdapter_DeviceDiscovered;

                // Always release the semaphore.
                _bleSemaphore.Release();
            }

            return _foundDevices;
        }

        /// <inheritdoc />
        public async Task<bool> SetColour(IDevice device, Color colour)
        {
            if (!await CheckPermission())
            {
                // If the app does not have Bluetooth permission, it cannot connect to Bluetooth
                // devices.
                // TODO: Better return here?
                return false;
            }
            if (device.State != DeviceState.Connected)
            {
                // The colour cannot be set on devices that are not connected.
                // TODO: Better return here?
                return false;
            }

            // Wait to enter the semaphore to ensure a single BLE operation runs at a time.
            await _bleSemaphore.WaitAsync();

            // Assume the operation failed, as there are many pathways for an error but only a
            // single code path for success.
            var sendSuccess = false;

            try
            {
                // TODO: Store the services and characteristics for each device so they only need
                // TODO: to be identified once?
                // Get the Nordic UART Service and RX characteristic from the device to send the
                // colour to.
                if (await device.GetServiceAsync(NordicUart.ServiceGuid) is { } uartService
                    && await uartService.GetCharacteristicAsync(NordicUart.RxCharacteristicGuid) is { CanWrite: true } rxCharacteristic)
                {
                    // TODO: Create BleColourPacket class?
                    // TODO: This is for testing. A proper packet structure needs to be agreed.
                    // Convert the selected colour to a hex value to transmit.
                    var payloadBytes = Encoding.UTF8.GetBytes(colour.ToHex());

                    // TODO: Only negotiate the MTU size once per connection.
                    // Try and negotiate the largest MTU size possible.
                    var currentMtu = await device.RequestMtuAsync(BleConstants.MaxMtuSize);

                    // In debug, ensure the packet can be transmitted with the given MTU.
                    Debug.Assert(payloadBytes.Length <= currentMtu - BleConstants.AttProtocolOverhead);

                    // Attempt to write the payload to the RX characteristic.
                    var writeResult = await rxCharacteristic.WriteAsync(payloadBytes);

                    if (writeResult == 0)
                    {
                        // A writeResult of 0 indicates success.
                        sendSuccess = true;
                    }
                    else
                    {
                        // TODO: Display popup with result code for debugging.
                        // TODO: Switch result to display error messages?
                        Debug.Assert(false);
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch all exceptions here and break in debug, as it is unexpected.
                Debug.Assert(false, ex.Message);
            }
            finally
            {
                // Always release the semaphore.
                _bleSemaphore.Release();
            }

            // Return whether the operation succeeded or not.
            return sendSuccess;
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
