using SuntoryManagementSystem_App.ViewModels;

namespace SuntoryManagementSystem_App.Pages;

public partial class ProductenPage : ContentPage
{
    public ProductenPage(ProductenViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
