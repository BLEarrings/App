namespace BLEarringController.Ble.Services
{
    /// <summary>
    /// Definitions for the
    /// <see href="https://docs.nordicsemi.com/bundle/ncs-latest/page/nrf/libraries/bluetooth/services/nus.html">
    /// Nordic UART Service</see>.
    /// </summary>
    internal static class NordicUart
    {
        #region Constants

        #region Internal

        /// <summary>
        /// The RX characteristic on the <see cref="ServiceId"/>. Data written to the RX
        /// Characteristic is sent to the UART interface.
        /// <para>
        /// Data can be sent to this characteristic with the <c>Write</c> or
        /// <c>Write Without Response</c> operation.
        /// </para>
        /// </summary>
        internal const string RxCharacteristicId = @"6E400002-B5A3-F393-E0A9-E50E24DCCA9E";

        /// <summary>
        /// The <see href="https://docs.nordicsemi.com/bundle/ncs-latest/page/nrf/libraries/bluetooth/services/nus.html#service_uuid">
        /// vendor-specific UUID for the Nordic Uart Service</see>.
        /// </summary>
        internal const string ServiceId = @"6E400001-B5A3-F393-E0A9-E50E24DCCA9E";

        /// <summary>
        /// The TX characteristic on the <see cref="ServiceId"/>. All data received over UART is
        /// sent over the TX characteristic as notifications.
        /// <para>
        /// Data can only be read from this characteristic using notifications.
        /// </para>
        /// </summary>
        internal const string TxCharacteristicId = @"6E400003-B5A3-F393-E0A9-E50E24DCCA9E";

        #endregion

        #endregion

        #region Fields

        #region Internal Static

        /// <inheritdoc cref="RxCharacteristicId"/>
        /// <remarks>
        /// This reflects the <see cref="RxCharacteristicId"/>, but as a <see cref="Guid"/>.
        /// </remarks>
        internal static Guid RxCharacteristicGuid = new(RxCharacteristicId);

        /// <inheritdoc cref="ServiceId"/>
        /// <remarks>
        /// This reflects the <see cref="ServiceId"/>, but as a <see cref="Guid"/>.
        /// </remarks>
        internal static Guid ServiceGuid = new(ServiceId);

        /// <inheritdoc cref="TxCharacteristicId"/>
        /// <remarks>
        /// This reflects the <see cref="TxCharacteristicId"/>, but as a <see cref="Guid"/>.
        /// </remarks>
        internal static Guid TxCharacteristicGuid = new(TxCharacteristicId);

        #endregion

        #endregion
    }
}
