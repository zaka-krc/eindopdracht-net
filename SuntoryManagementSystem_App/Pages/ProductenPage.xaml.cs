using SuntoryManagementSystem_App.ViewModels;
using SuntoryManagementSystem.Models;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.Pages;

public partial class ProductenPage : ContentPage
{
    private readonly ProductenViewModel _viewModel;
    
    public ProductenPage(ProductenViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        Debug.WriteLine("ProductenPage: Initialized with ViewModel");
        Debug.WriteLine($"ProductenPage: ViewModel type = {viewModel.GetType().Name}");
        Debug.WriteLine($"ProductenPage: ViewModel commands:");
        Debug.WriteLine($"  - VoegProductToeCommand: {(viewModel.VoegProductToeCommand != null ? "OK" : "NULL")}");
        Debug.WriteLine($"  - BekijkProductCommand: {(viewModel.BekijkProductCommand != null ? "OK" : "NULL")}");
        Debug.WriteLine($"  - BewerkProductCommand: {(viewModel.BewerkProductCommand != null ? "OK" : "NULL")}");
        Debug.WriteLine($"  - VerwijderProductCommand: {(viewModel.VerwijderProductCommand != null ? "OK" : "NULL")}");
        Debug.WriteLine($"  - RefreshCommand: {(viewModel.RefreshCommand != null ? "OK" : "NULL")}");
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("ProductenPage: OnAppearing called");
        Debug.WriteLine($"ProductenPage: Gefilterd Producten count = {_viewModel.GefilterdProducten.Count}");
    }
    
    // Card tap event handler
    private async void OnCardTapped(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnCardTapped: Card tapped");
            
            if (sender is Grid grid && grid.BindingContext is Product product)
            {
                Debug.WriteLine($"OnCardTapped: Product = {product.ProductName}");
                await _viewModel.BekijkProductCommand.ExecuteAsync(product);
            }
            else
            {
                Debug.WriteLine("OnCardTapped: Could not get product from BindingContext");
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
            await _viewModel.VoegProductToeCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnNieuwClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    // Product card button event handlers
    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnDetailsClicked: Button clicked");
            
            if (sender is Button button && button.BindingContext is Product product)
            {
                Debug.WriteLine($"OnDetailsClicked: Product = {product.ProductName}");
                await _viewModel.BekijkProductCommand.ExecuteAsync(product);
            }
            else
            {
                Debug.WriteLine("OnDetailsClicked: Could not get product from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnDetailsClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    private async void OnBewerkClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnBewerkClicked: Button clicked");
            
            if (sender is Button button && button.BindingContext is Product product)
            {
                Debug.WriteLine($"OnBewerkClicked: Product = {product.ProductName}");
                await _viewModel.BewerkProductCommand.ExecuteAsync(product);
            }
            else
            {
                Debug.WriteLine("OnBewerkClicked: Could not get product from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnBewerkClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    private async void OnVerwijderClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnVerwijderClicked: Button clicked");
            
            if (sender is Button button && button.BindingContext is Product product)
            {
                Debug.WriteLine($"OnVerwijderClicked: Product = {product.ProductName}");
                await _viewModel.VerwijderProductCommand.ExecuteAsync(product);
            }
            else
            {
                Debug.WriteLine("OnVerwijderClicked: Could not get product from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnVerwijderClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    // SwipeItem event handlers
    private async void OnSwipeBewerkInvoked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnSwipeBewerkInvoked: SwipeItem invoked");
            
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Product product)
            {
                Debug.WriteLine($"OnSwipeBewerkInvoked: Product = {product.ProductName}");
                await _viewModel.BewerkProductCommand.ExecuteAsync(product);
            }
            else
            {
                Debug.WriteLine("OnSwipeBewerkInvoked: Could not get product from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnSwipeBewerkInvoked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    private async void OnSwipeVerwijderInvoked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnSwipeVerwijderInvoked: SwipeItem invoked");
            
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Product product)
            {
                Debug.WriteLine($"OnSwipeVerwijderInvoked: Product = {product.ProductName}");
                await _viewModel.VerwijderProductCommand.ExecuteAsync(product);
            }
            else
            {
                Debug.WriteLine("OnSwipeVerwijderInvoked: Could not get product from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnSwipeVerwijderInvoked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
}
