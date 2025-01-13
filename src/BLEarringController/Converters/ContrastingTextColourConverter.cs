using System.Globalization;

namespace BLEarringController.Converters
{
    /// <summary>
    /// <see cref="IValueConverter"/> to convert a background <see cref="Color"/> to a standard
    /// text <see cref="Color"/> that has the best contrast against the background.
    /// </summary>
    public class ContrastingTextColourConverter : IValueConverter
    {
        #region Constants

        #region Private

        /// <summary>
        /// The luma coefficient for blue for formats following the
        /// <see href="https://en.wikipedia.org/wiki/Rec._709">ITU-R Recommendation 709</see>.
        /// </summary>
        private const double BlueLumaCoefficient = 0.0722;

        /// <summary>
        /// The luma coefficient for green for formats following the
        /// <see href="https://en.wikipedia.org/wiki/Rec._709">ITU-R Recommendation 709</see>.
        /// </summary>
        private const double GreenLumaCoefficient = 0.7152;

        /// <summary>
        /// The value representing the midpoint of the luma range. Values greater than this are
        /// considered "bright" and values lower than this are considered "dark".
        /// </summary>
        private const double LumaMidpoint = 0.5;

        /// <summary>
        /// The luma coefficient for red for formats following the
        /// <see href="https://en.wikipedia.org/wiki/Rec._709">ITU-R Recommendation 709</see>.
        /// </summary>
        private const double RedLumaCoefficient = 0.2126;

        #endregion

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Convert a background <see cref="Color"/> to a text <see cref="Color"/> that should
        /// contrast the background. Note that this only uses <see cref="Colors.Black"/> and
        /// <see cref="Colors.White"/>, based on the luma of the background.
        /// </summary>
        /// <param name="value">
        /// The background <see cref="Color"/> that the text is displayed against.
        /// </param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>
        /// Either <see cref="Colors.Black"/> or <see cref="Colors.White"/>, depending on which has
        /// the best contrast against the background.
        /// </returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Color backgroundColour)
            {
                // Return null if the provided object was not a colour.
                return null;
            }

            // Calculate the "luma" of the background colour, assuming the colour is represented
            // using Rec. 709 colour representation.
            //
            // Note that the red, green and blue values are returned as floats between 0 and 1, so
            // the "luma" value will also be between 0 and 1.
            var luma = backgroundColour.Red * RedLumaCoefficient
                       + backgroundColour.Green * GreenLumaCoefficient
                       + backgroundColour.Blue * BlueLumaCoefficient;

            // Check the luma value against the midpoint of the luma range. If it is "lighter"
            // (i.e. greater) than the midpoint, the text should be dark. If it is "darker" (i.e.
            // less than) the midpoint, the text should be light.
            return luma > LumaMidpoint ? Colors.Black : Colors.White;

        }

        /// <summary>
        /// This method is not implemented, it is not possible to convert back to the background
        /// colour.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">
        /// This method will throw a <see cref="NotImplementedException"/>, as it is not
        /// implemented.
        /// </exception>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}