using SuntoryManagementSystem_App.ViewModels;

namespace SuntoryManagementSystem_App.Pages;

public partial class CustomerPage : ContentPage
{
    public CustomerPage(CustomerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
