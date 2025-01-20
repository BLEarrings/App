using BLEarringController.ViewModels;

namespace BLEarringController.Views
{
    public partial class BleScanView : ContentPage
    {
        #region Construction

        public BleScanView(BleScanViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }

        #endregion
    }
}