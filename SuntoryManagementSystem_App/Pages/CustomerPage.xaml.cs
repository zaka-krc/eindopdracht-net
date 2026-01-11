using SuntoryManagementSystem_App.ViewModels;
using SuntoryManagementSystem.Models;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.Pages;

public partial class CustomerPage : ContentPage
{
    private readonly CustomerViewModel _viewModel;
    
    public CustomerPage(CustomerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        Debug.WriteLine("CustomerPage: Initialized with ViewModel");
    }
    
    // Header button event handlers
    private async void OnAddCustomerClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnAddCustomerClicked: Button clicked");
            await _viewModel.AddCustomerCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnAddCustomerClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnRefreshClicked: Button clicked");
            await _viewModel.RefreshCustomersCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnRefreshClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    private async void OnRefreshViewRefreshing(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnRefreshViewRefreshing: RefreshView triggered");
            await _viewModel.RefreshCustomersCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnRefreshViewRefreshing ERROR: {ex.Message}");
        }
    }
    
    // Card tap event handler
    private async void OnCustomerCardTapped(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnCustomerCardTapped: Card tapped");
            
            if (sender is Border border && border.BindingContext is Customer customer)
            {
                Debug.WriteLine($"OnCustomerCardTapped: Customer = {customer.CustomerName}");
                await _viewModel.ViewCustomerCommand.ExecuteAsync(customer);
            }
            else
            {
                Debug.WriteLine("OnCustomerCardTapped: Could not get customer from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnCustomerCardTapped ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    // Customer action button event handlers
    private async void OnEditCustomerClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnEditCustomerClicked: Button clicked");
            
            if (sender is Button button && button.BindingContext is Customer customer)
            {
                Debug.WriteLine($"OnEditCustomerClicked: Customer = {customer.CustomerName}");
                await _viewModel.EditCustomerCommand.ExecuteAsync(customer);
            }
            else
            {
                Debug.WriteLine("OnEditCustomerClicked: Could not get customer from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnEditCustomerClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
    
    private async void OnDeleteCustomerClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("OnDeleteCustomerClicked: Button clicked");
            
            if (sender is Button button && button.BindingContext is Customer customer)
            {
                Debug.WriteLine($"OnDeleteCustomerClicked: Customer = {customer.CustomerName}");
                await _viewModel.DeleteCustomerCommand.ExecuteAsync(customer);
            }
            else
            {
                Debug.WriteLine("OnDeleteCustomerClicked: Could not get customer from BindingContext");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnDeleteCustomerClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout: {ex.Message}", "OK");
        }
    }
}
