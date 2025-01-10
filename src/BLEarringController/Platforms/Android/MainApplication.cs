using Android.App;
using Android.Runtime;

namespace BLEarringController
{
    [Application]
    public class MainApplication : MauiApplication
    {
        #region Construction

        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        #endregion

        #region Methods

        #region Protected

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        #endregion

        #endregion
    }
}
