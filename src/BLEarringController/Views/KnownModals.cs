using Plugin.BLE.Abstractions.Contracts;

namespace BLEarringController.Views
{
    internal static class KnownModals
    {
        #region Constants

        #region Internal

        /// <summary>
        /// A modal that allows scanning for Bluetooth LE devices, and allows the user to select
        /// one that they would like to connect to.
        /// </summary>
        /// <remarks>
        /// This view will navigate backwards once a device has been selected, and return the
        /// selected <see cref="IDevice"/> in the <see cref="ShellNavigationQueryParameters"/>
        /// under the <see cref="NavigationQueryKeys.SelectedBleDeviceKey"/> key.
        /// </remarks>
        internal const string BleScanView = @"blescanview";

        #endregion

        #endregion
    }
}
