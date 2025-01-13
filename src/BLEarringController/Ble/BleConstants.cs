namespace BLEarringController.Ble
{
    /// <summary>
    /// A class containing constants related to Bluetooth Low Energy.
    /// </summary>
    internal static class BleConstants
    {
        #region Constants

        #region Internal

        /// <summary>
        /// The number of bytes in each BLE packet that are used by the ATT protocol. Any MTU size
        /// will include this overhead.
        /// </summary>
        internal const int AttProtocolOverhead = 3;

        /// <summary>
        /// The absolute maximum MTU size for BLE devices, as defined in the Bluetooth Core
        /// Specification v5.2, Vol.3, Part F, 3.2.9: "The maximum length of an attribute value
        /// shall be 512 octets.".
        /// </summary>
        /// <remarks>
        /// Note that this includes the <see cref="AttProtocolOverhead"/>.
        /// </remarks>
        internal const int MaxMtuSize = 512;

        #endregion

        #endregion
    }
}
