namespace BLEarringController
{
    public partial class App : Application
    {
        #region Construction

        public App()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        #region Protected

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        #endregion

        #endregion
    }
}