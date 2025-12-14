using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_App.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.ViewModels;

public partial class CustomerViewModel : ObservableObject
{
    private readonly LocalDbContext _context;

    [ObservableProperty]
    private ObservableCollection<Customer> customers = new();

    [ObservableProperty]
    private ObservableCollection<Customer> filteredCustomers = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedTypeFilter = "Alle";

    [ObservableProperty]
    private string selectedStatusFilter = "Alle";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool isRefreshing = false;

    public CustomerViewModel(LocalDbContext context)
    {
        _context = context;
        _ = LoadCustomersAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedTypeFilterChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = Customers.AsEnumerable();

        // Filter op zoekterm
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(c =>
                c.CustomerName.ToLower().Contains(search) ||
                (c.Email?.ToLower().Contains(search) ?? false) ||
                (c.City?.ToLower().Contains(search) ?? false) ||
                (c.ContactPerson?.ToLower().Contains(search) ?? false));
        }

        // Filter op type
        if (SelectedTypeFilter != "Alle")
        {
            filtered = filtered.Where(c => c.CustomerType == SelectedTypeFilter);
        }

        // Filter op status
        if (SelectedStatusFilter != "Alle")
        {
            filtered = filtered.Where(c => c.Status == SelectedStatusFilter);
        }

        FilteredCustomers = new ObservableCollection<Customer>(filtered);
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            Debug.WriteLine("Loading customers from database...");

            var customerList = await _context.Customers
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.CustomerName)
                .ToListAsync();

            Customers = new ObservableCollection<Customer>(customerList);
            ApplyFilters();

            Debug.WriteLine($"Loaded {Customers.Count} customers");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading customers: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kan klanten niet laden: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshCustomersAsync()
    {
        if (IsRefreshing) return;

        try
        {
            IsRefreshing = true;
            await LoadCustomersAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task AddCustomerAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("CustomerDetailPage");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to CustomerDetailPage: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", "Kan niet navigeren naar detail pagina", "OK");
        }
    }

    [RelayCommand]
    private async Task ViewCustomerAsync(Customer customer)
    {
        if (customer == null) return;

        try
        {
            await Shell.Current.GoToAsync($"CustomerDetailPage?CustomerId={customer.CustomerId}&ViewMode=true");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to customer detail: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", "Kan klant niet openen", "OK");
        }
    }

    [RelayCommand]
    private async Task EditCustomerAsync(Customer customer)
    {
        if (customer == null) return;

        try
        {
            await Shell.Current.GoToAsync($"CustomerDetailPage?CustomerId={customer.CustomerId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to edit customer: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", "Kan klant niet bewerken", "OK");
        }
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync(Customer customer)
    {
        if (customer == null) return;

        var confirm = await Shell.Current.DisplayAlert(
            "Verwijderen",
            $"Weet je zeker dat je klant '{customer.CustomerName}' wilt verwijderen?",
            "Ja",
            "Nee");

        if (!confirm) return;

        try
        {
            // Soft delete
            customer.IsDeleted = true;
            customer.DeletedDate = DateTime.Now;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            await LoadCustomersAsync();
            await Shell.Current.DisplayAlert("Succes", "Klant verwijderd", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting customer: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kan klant niet verwijderen: {ex.Message}", "OK");
        }
    }
}
