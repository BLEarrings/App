namespace BLEarringController.ViewModels
{
    public sealed class HomePageViewModel : ViewModelBase
    {
        #region Properties

        #region Public

        /// <summary>
        /// The subtitle to display underneath the <see cref="Title"/>.
        /// </summary>
        public string Subtitle => "Wow... this is a very empty home page! Must be a placeholder...";

        /// <summary>
        /// The title to display at the top of the page.
        /// </summary>
        public string Title => "Home Page";

        #endregion

        #endregion
    }
}
