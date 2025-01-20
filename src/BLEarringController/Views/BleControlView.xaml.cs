using BLEarringController.ViewModels;

namespace BLEarringController.Views
{
    public partial class BleControlView : ContentPage
    {
        #region Construction

        public BleControlView(BleControlViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }

        #endregion
    }
}