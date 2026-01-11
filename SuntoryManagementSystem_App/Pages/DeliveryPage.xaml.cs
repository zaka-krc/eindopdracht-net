using SuntoryManagementSystem_App.ViewModels;
using SuntoryManagementSystem.Models;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.Pages;

public partial class DeliveryPage : ContentPage
{
    private readonly DeliveryViewModel _viewModel;
    
    public DeliveryPage(ViewModels.DeliveryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        Debug.WriteLine("DeliveryPage: Initialized with ViewModel");
        Debug.WriteLine($"DeliveryPage: ViewModel type = {viewModel.GetType().Name}");
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("DeliveryPage: OnAppearing called");
        Debug.WriteLine($"DeliveryPage: GefilterdeLeveringen count = {_viewModel.GefilterdeLeveringen.Count}");
    }
    
    // Card tap event handler
    private async void OnCardTapped(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnCardTapped: Card tapped");
            
            if (sender is Border border && border.BindingContext is Delivery delivery)
            {
                Debug.WriteLine($"OnCardTapped: Delivery = {delivery.ReferenceNumber}");
                await _viewModel.BekijkLeveringCommand.ExecuteAsync(delivery);
            }
            else
            {
                Debug.WriteLine("OnCardTapped: Could not get delivery from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnCardTapped ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    // Header button event handlers
    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnRefreshClicked: Button clicked");
            await _viewModel.RefreshCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnRefreshClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    private async void OnNieuwClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnNieuwClicked: Button clicked");
            await _viewModel.VoegLeveringToeCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnNieuwClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    // Delivery card button event handlers
    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnDetailsClicked: Button clicked");
            
            if (sender is Button button && button.BindingContext is Delivery delivery)
            {
                Debug.WriteLine($"OnDetailsClicked: Delivery = {delivery.ReferenceNumber}");
                await _viewModel.BewerkLeveringCommand.ExecuteAsync(delivery);
            }
            else
            {
                Debug.WriteLine("OnDetailsClicked: Could not get delivery from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnDetailsClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    private async void OnVerwerkClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnVerwerkClicked: Button clicked");
            
            if (sender is Button button && button.BindingContext is Delivery delivery)
            {
                Debug.WriteLine($"OnVerwerkClicked: Delivery = {delivery.ReferenceNumber}");
                await _viewModel.VerwerkLeveringCommand.ExecuteAsync(delivery);
            }
            else
            {
                Debug.WriteLine("OnVerwerkClicked: Could not get delivery from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnVerwerkClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    // Delete button event handler (changed from SwipeItem to regular Button)
    private async void OnSwipeVerwijderInvoked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnSwipeVerwijderInvoked: Button clicked");
            
            if (sender is Button button && button.BindingContext is Delivery delivery)
            {
                Debug.WriteLine($"OnSwipeVerwijderInvoked: Delivery = {delivery.ReferenceNumber}");
                await _viewModel.VerwijderLeveringCommand.ExecuteAsync(delivery);
            }
            else
            {
                Debug.WriteLine("OnSwipeVerwijderInvoked: Could not get delivery from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnSwipeVerwijderInvoked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }

    private async void OnRefreshViewRefreshing(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnRefreshViewRefreshing: RefreshView triggered");
            await _viewModel.RefreshCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnRefreshViewRefreshing ERROR: {ex.Message}");
        }
    }
}
