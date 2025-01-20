using Plugin.BLE.Abstractions.Contracts;

namespace BLEarringController.Services
{
    /// <summary>
    /// A singleton service that allows controlled interaction with BLE devices using the
    /// <see cref="Plugin.BLE"/> library. This allows for central implementations of BLE logic, and
    /// allows some restrictions
    /// <see href="https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le?tab=readme-ov-file#caution-important-remarks--api-limitations">
    /// listed on the GitHub page for the library</see> to be adhered to:
    /// <list type="bullet">
    ///     <item>
    ///     <description>
    ///     <see cref="ICharacteristic.WriteAsync"/> must be called from the main thread.
    ///     </description>
    ///     </item>
    ///     <item>
    ///     <description>
    ///     BLE commands must be sequential, so the previous command must be allowed to finish
    ///     before starting the next.
    ///     </description>
    ///     </item>
    /// </list>
    /// Managing all BLE interactions centrally in a singleton allows the class to ensure that all
    /// commands are sequential, as wall as allowing an easier guarantee that methods are run on
    /// the main thread when required.
    /// </summary>
    public interface IBleManager : ISingletonService
    {
        #region Constants

        #region Public

        /// <summary>
        /// The default timeout in milliseconds for a BLE scan.
        /// </summary>
        /// <remarks>
        /// This is the value used by the <see cref="Scan()"/> method.
        /// </remarks>
        public const int DefaultScanTimeout = 500;

        #endregion

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Attempt to bond (pair) to a BLE device.
        /// </summary>
        /// <param name="device">The <see cref="IDevice"/> to try and bond to.</param>
        /// <returns>
        /// A <see cref="Task"/> that will be completed with a <see cref="bool"/> representing the
        /// bonding success when the operation is completed. <c>true</c> indicates that the device
        /// bonded successfully. <c>false</c> otherwise.
        /// </returns>
        public Task<bool> Bond(IDevice device);

        /// <summary>
        /// Check whether the app already has permission to use the device's Bluetooth hardware
        /// without requesting it. This can be used as a lightweight check before BLE operations.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which will be completed with <c>true</c> if the app already has
        /// permission to use the BLE hardware, and <c>false</c> otherwise.
        /// </returns>
        public Task<bool> CheckPermission();

        /// <summary>
        /// Attempt to connect to a BLE device.
        /// </summary>
        /// <param name="device">The <see cref="IDevice"/> to try and connect to.</param>
        /// <returns>
        /// A <see cref="Task"/> that will be completed with a <see cref="bool"/> representing the
        /// connection success when the operation is completed. <c>true</c> indicates that the
        /// device connected successfully. <c>false</c> otherwise.
        /// </returns>
        public Task<bool> Connect(IDevice device);

        /// <summary>
        /// Check whether the app has permission to use the device's Bluetooth hardware and if not,
        /// request the permission from the user. If the user does need to grant the app
        /// permission, an alert will be used to explain the request in line with the
        /// <see href="https://developer.android.com/training/permissions/usage-notes">Android App
        /// Permission Best Practices</see>.
        /// </summary>
        /// <remarks>
        /// This may also prompt a location permission on some devices, since the location of
        /// Bluetooth devices can be used to estimate location.
        /// </remarks>
        /// <param name="message">
        /// The message that will be used to explain to the user contextually why the Bluetooth
        /// permission is being requested, in the event the user has to be prompted for permission.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which will be completed once the user selects whether to grant the
        /// app permission or not. <c>true</c> is used to complete the task if permission is
        /// granted, <c>false</c> is used otherwise.
        /// </returns>
        public Task<bool> RequestPermission(string message);

        /// <summary>
        /// Scan for supported BLE devices using the <see cref="DefaultScanTimeout"/>.
        /// </summary>
        /// <remarks>
        /// To change the duration of the scan, use <see cref="Scan(int)"/>.
        /// </remarks>
        /// <returns>
        /// A <see cref="Task"/> that is completed with a <see cref="List{T}"/> of
        /// <see cref="IDevice"/>s found after the scan completes.
        /// </returns>
        public Task<List<IDevice>> Scan();

        /// <summary>
        /// Scan for supported BLE devices for the specified amount of time.
        /// </summary>
        /// <remarks>
        /// To use the <see cref="DefaultScanTimeout"/>, use <see cref="Scan()"/>.
        /// </remarks>
        /// <param name="timeout">The amount of time in milliseconds to run the scan for.</param>
        /// <returns>
        /// A <see cref="Task"/> that is completed with a <see cref="List{T}"/> of
        /// <see cref="IDevice"/>s found after the scan completes.
        /// </returns>
        public Task<List<IDevice>> Scan(int timeout);

        /// <summary>
        /// Attempt to set the <see cref="Color"/> displayed on a BLE device.
        /// </summary>
        /// <param name="device">
        /// The <see cref="IDevice"/> to set the <see cref="Color"/> of.
        /// </param>
        /// <param name="colour">The <see cref="Color"/> to set the BLE device to.</param>
        /// <returns>
        /// A <see cref="Task"/> which will be completed when the operation finishes with a
        /// <see cref="bool"/> representing the success of the operation. <c>true</c> represents
        /// success, and <c>false</c> indicates failure.
        /// </returns>
        public Task<bool> SetColour(IDevice device, Color colour);

        #endregion

        #endregion
    }
}
