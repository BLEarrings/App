using System.Globalization;
using System.Text.RegularExpressions;

namespace BLEarringController.Converters
{
    /// <summary>
    /// A <see cref="IValueConverter"/> implementation to convert the <see cref="Guid"/> MAC
    /// address representation used in the <see cref="Plugin.BLE"/> library, to the standard MAC
    /// address <see cref="string"/> representation for display.
    /// </summary>
    /// <remarks>
    /// The <see cref="ConvertBack(object?, Type, object?, CultureInfo)"/> method is not currently
    /// implemented.
    /// </remarks>
    public sealed partial class GuidToMacStringConverter : IValueConverter
    {
        #region Constants

        #region Private

        /// <summary>
        /// The <see cref="Regex"/> expression used to identify a valid <see cref="Guid"/> MAC
        /// address.
        /// </summary>
        /// <remarks>
        /// This stores the 12 characters of the MAC address in a named capture group called
        /// <c>macString</c>.
        /// </remarks>
        private const string GuidMacExpression = @"00000000-0000-0000-0000-(?<macString>[0-9a-fA-F]{12})";

        /// <summary>
        /// The number of hex digits in an octet.
        /// </summary>
        private const int OctetLength = 2;

        /// <summary>
        /// The <see cref="char"/> to use to separate the octets in the final converted
        /// <see cref="string"/>.
        /// </summary>
        private const char OctetSeparator = ':';

        #endregion

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Convert a <see cref="Guid"/> representing a MAC address into a <see cref="string"/> MAC
        /// address representation.
        /// </summary>
        /// <remarks>
        /// The <see cref="Guid"/> must contain zeros everywhere except the node, which should
        /// contain the MAC address. I.e. <c>00000000-0000-0000-0000-AABBCCDDEEFF</c> representing
        /// the MAC address <c>AA:BB:CC:DD:EE:FF</c>.
        /// </remarks>
        /// <param name="value">
        /// The <see cref="Guid"/> containing the MAC address to convert.
        /// </param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>
        /// A <see cref="string"/> representing the MAC address contained within the provided
        /// <see cref="Guid"/>, or <c>null</c> if the value could not be converted.
        /// </returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Guid guid)
            {
                // Only Guid values can be converted.
                return null;
            }
            if (GuidMacRegex().Match(guid.ToString()) is not { Success: true } regexMatch)
            {
                // Value cannot be converted if the Guid is not in the expected format, containing
                // all zeros except the "node".
                return null;
            }

            var macString = regexMatch.Groups["macString"].Value;

            // The macString contains all the characters of the MAC address, but the octets are not
            // separated. Split the string into the octets so the separators can be added.
            
            // Generate a sequence of numbers from 0 to half the length of the MAC string. These
            // will represent the "index" of each octet.
            var octets = Enumerable.Range(0, macString.Length / OctetLength)
                // For each octetIndex, return a substring from the macString starting from the
                // index of the first character in the octet (octetIndex * 2 as each octet is 2
                // chartacters long), and having a length of 2 for the complete octet.
                .Select(octetIndex => macString.Substring(octetIndex * OctetLength, OctetLength));

            // Join the octets using the separator, ensuring the entire string is uppercase for
            // consistency, to form the final MAC address.
            return string.Join(OctetSeparator, octets).ToUpper();
        }

        /// <summary>
        /// Convert a <see cref="string"/> MAC address into <see cref="Guid"/> MAC address
        /// representation.
        /// </summary>
        /// <remarks>
        /// This method is not yet implemented.
        /// </remarks>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">
        /// This method is not yet implemented.
        /// </exception>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException(
                @"The GuidToMacStringConverter.ConvertBack() method has not yet been implemented.");
        }

        #endregion

        #region Private Static

        /// <inheritdoc />
        [GeneratedRegex(GuidMacExpression)]
        private static partial Regex GuidMacRegex();

        #endregion

        #endregion
    }
}
