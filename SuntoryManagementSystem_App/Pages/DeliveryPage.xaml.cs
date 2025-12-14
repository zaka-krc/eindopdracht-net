namespace SuntoryManagementSystem_App.Pages;

public partial class DeliveryPage : ContentPage
{
    public DeliveryPage(ViewModels.DeliveryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
