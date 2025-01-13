namespace BLEarringController.Controls
{
    /// <summary>
    /// A custom control to create a "card" with a fixed aspect ratio to place contents in. The
    /// card will always take the full width of the parent container, and will adjust its height to
    /// display with the correct aspect ratio. Note that the height will still be constrained by
    /// the size of the parent container, so the aspect ratio will not be maintained if the parent
    /// container does not have enough space available.
    /// <para>
    /// The corner radius and background colour of the card are also customisable.
    /// </para>
    /// The card can have a <see cref="Command"/> supplied, which will be executed when any area on
    /// the card is tapped by the user.
    /// </summary>
    /// <remarks>
    /// The aspect ratio defaults to 16:9. It has a minimum value of 1:1, and a maximum value of
    /// 3:1.
    /// </remarks>
    public partial class FixedAspectCard : ContentView
    {
        #region Constants

        #region Private

        /// <summary>
        /// The default aspect ratio is 16:9.
        /// </summary>
        private const double DefaultAspectRatio = 16d / 9d;

        /// <summary>
        /// The maximum aspect ratio is 3:1.
        /// </summary>
        private const double MaxAspectRatio = 3;

        /// <summary>
        /// The minimum aspect ratio is 1:1.
        /// </summary>
        private const double MinAspectRatio = 1;

        /// <summary>
        /// By default, the card is transparent.
        /// </summary>
        private static readonly Color s_defaultCardBackgroundColor = Colors.Transparent;

        /// <summary>
        /// The default corner radius for the card.
        /// </summary>
        private static readonly CornerRadius s_defaultCornerRadius = new(20);

        #endregion

        #region Public

        /// <summary>
        /// The <see cref="BindableProperty"/> for the <see cref="CardBackgroundColor"/>.
        /// </summary>
        public static readonly BindableProperty CardBackgroundColorProperty =
            BindableProperty.Create(nameof(CardBackgroundColor), typeof(Color), typeof(FixedAspectCard), s_defaultCardBackgroundColor);

        /// <summary>
        /// The <see cref="BindableProperty"/> for the <see cref="CornerRadius"/>.
        /// </summary>
        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create(nameof(CornerRadius), typeof(CornerRadius), typeof(FixedAspectCard), s_defaultCornerRadius);

        /// <summary>
        /// The <see cref="BindableProperty"/> for the <see cref="AspectRatio"/>.
        /// </summary>
        public static readonly BindableProperty AspectRatioProperty =
            BindableProperty.Create(nameof(AspectRatio), typeof(double), typeof(FixedAspectCard), DefaultAspectRatio, BindingMode.Default, ValidateAspectRatio, OnAspectRatioChanged);


        /// <summary>
        /// The <see cref="BindableProperty"/> for the <see cref="CardTappedCommand"/>.
        /// </summary>
        public static readonly BindableProperty CardTappedCommandProperty =
            BindableProperty.Create(nameof(CardTappedCommand), typeof(Command), typeof(FixedAspectCard));

        #endregion

        #endregion

        #region Construction

        public FixedAspectCard()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        #region Public

        /// <summary>
        /// The desired aspect ratio for the card. The aspect ratio is represented by a
        /// <see cref="double"/> which is calculated by dividing the width by the height of the
        /// desired ratio. For example, a ratio of <c>16:9</c> would be set using the
        /// <see cref="double"/> <c>1.7777...8</c>.
        /// </summary>
        public double AspectRatio
        {
            get => (double)GetValue(AspectRatioProperty);
            set => SetValue(AspectRatioProperty, value);
        }

        /// <summary>
        /// The background <see cref="Color"/> for the card.
        /// </summary>
        public Color CardBackgroundColor
        {
            get => (Color)GetValue(CardBackgroundColorProperty);
            set => SetValue(CardBackgroundColorProperty, value);
        }

        /// <summary>
        /// The <see cref="Command"/> that will be executed when anywhere on the card is tapped.
        /// </summary>
        public Command? CardTappedCommand
        {
            get => (Command)GetValue(CardTappedCommandProperty);
            set => SetValue(CardTappedCommandProperty, value);
        }

        /// <summary>
        /// The <see cref="CornerRadius"/> for the card.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #endregion

        #region Methods

        #region Protected

        /// <inheritdoc />
        /// <remarks>
        /// This override attempts to maintain the <see cref="AspectRatio"/> of the card during
        /// each measure pass. However, this does conform to the height constraint so the aspect
        /// ratio may not be maintained if the parent container is not large enough.
        /// </remarks>
        protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
        {
            // Calculate the desired height to maintain the aspect ratio, based on the maximum
            // available width.
            var desiredHeight = widthConstraint / AspectRatio;

            // Constrain the height value to the height of the parent container.
            desiredHeight = Math.Min(desiredHeight, heightConstraint);

            // Return the desired size for the control, constrained by the width and height of the
            // parent container.
            return new Size(widthConstraint, desiredHeight);
        }

        #endregion

        #region Private Static

        /// <summary>
        /// A delegate that is called when the <see cref="AspectRatioProperty"/> is changed. This
        /// invokes <see cref="VisualElement.InvalidateMeasure"/> on the
        /// <see cref="BindableObject"/> if it is a <see cref="FixedAspectCard"/>, so that the
        /// <see cref="MeasureOverride"/> is called to re-calculate the size of the card.
        /// </summary>
        /// <param name="bindable">
        /// The object that owns the <see cref="BindableProperty"/> that changed.
        /// </param>
        /// <param name="oldValue">The old <see cref="AspectRatio"/> value.</param>
        /// <param name="newValue">The new <see cref="AspectRatio"/> value.</param>
        private static void OnAspectRatioChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // Ensure the BindableObject is a FixedAspectCard.
            if (bindable is FixedAspectCard card)
            {
                // Call InvalidateMeasure() on the FixedAspectCard, to cause a measure pass so that
                // the new aspect ratio is reflected by the card.
                card.InvalidateMeasure();
            }
        }

        /// <summary>
        /// A delegate that is called to validate values of the <see cref="AspectRatioProperty"/>.
        /// This ensures that the <see cref="AspectRatio"/> can only be set to values between
        /// <see cref="MinAspectRatio"/> and <see cref="MaxAspectRatio"/>.
        /// </summary>
        /// <param name="bindable">
        /// The object that owns the <see cref="BindableProperty"/> that the value is being
        /// validated for.
        /// </param>
        /// <param name="value">The value to validate.</param>
        /// <returns></returns>
        private static bool ValidateAspectRatio(BindableObject bindable, object value)
        {
            // The AspectRatio must be a double. Any other values are invalid.
            if (value is not double aspectRatio)
            {
                return false;
            }

            // The AspectRatio is valid if it is between MinAspectRatio and MaxAspectRatio.
            return aspectRatio is >= MinAspectRatio and <= MaxAspectRatio;
        }

        #endregion

        #endregion
    }
}