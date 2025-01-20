using BLEarringController.ViewModels;

namespace BLEarringController.Views
{
    public partial class HomePageView : ContentPage
    {
        #region Construction

        public HomePageView(HomePageViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }

        #endregion
    }
}
