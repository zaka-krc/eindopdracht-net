using SuntoryManagementSystem_App.ViewModels;

namespace SuntoryManagementSystem_App.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
