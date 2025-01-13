using BLEarringController.Views;

namespace BLEarringController
{
    public partial class AppShell : Shell
    {
        #region Construction

        public AppShell()
        {
            InitializeComponent();

            RegisterNavigationRoutes();
        }

        #endregion

        #region Methods

        #region Private Static

        /// <summary>
        /// Register all relative navigation routes in the app, to allow navigation to views which
        /// cannot be navigated to from the flyout.
        /// </summary>
        /// <remarks>
        /// This is usually used for modal views, which need to be navigated to from another view.
        /// </remarks>
        private static void RegisterNavigationRoutes()
        {
            // The BleControlView should be able to navigate to the BleScanView modal to select a
            // device to control.
            Routing.RegisterRoute($"{KnownViews.BleControlView}/{KnownModals.BleScanView}", typeof(BleScanView));
        }

        #endregion

        #endregion
    }
}
