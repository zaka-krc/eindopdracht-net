using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SuntoryManagementSystem_App.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly LocalDbContext _context;
    
    // User info
    [ObservableProperty]
    private string gebruikersNaam = "Admin";
    
    [ObservableProperty]
    private string gebruikersRol = "Administrator";
    
    // Sync info
    [ObservableProperty]
    private string laatsteSyncTijd = "Nog niet gesynchroniseerd";
    
    [ObservableProperty]
    private bool isSyncing = false;
    
    // Dashboard stats
    [ObservableProperty]
    private int aantalProducten = 0;
    
    [ObservableProperty]
    private int lageVoorraadCount = 0;
    
    [ObservableProperty]
    private int openstaandeLeveringen = 0;
    
    [ObservableProperty]
    private int actieveKlanten = 0;
    
    // Waarschuwingen lijst
    [ObservableProperty]
    private ObservableCollection<string> voorraadWaarschuwingen = new();
    
    // Constructor
    public MainViewModel(LocalDbContext context)
    {
        _context = context;
        _ = LoadDataAsync();
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            Debug.WriteLine("MainViewModel: Loading dashboard data...");
            
            // Haal ECHTE stats uit database
            AantalProducten = await _context.Products.CountAsync(p => !p.IsDeleted);
            LageVoorraadCount = await _context.StockAlerts.CountAsync(a => a.Status == "Active" && !a.IsDeleted);
            OpenstaandeLeveringen = await _context.Deliveries.CountAsync(d => !d.IsDeleted && d.Status == "Gepland");
            ActieveKlanten = await _context.Customers.CountAsync(c => !c.IsDeleted && c.Status == "Active");
            
            Debug.WriteLine($"Stats loaded: Products={AantalProducten}, LowStock={LageVoorraadCount}, Deliveries={OpenstaandeLeveringen}, Customers={ActieveKlanten}");

            // Haal low stock producten op
            var lowStockProducts = await _context.Products
                .Where(p => !p.IsDeleted && p.IsActive && p.StockQuantity < p.MinimumStock)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .Select(p => $"{p.ProductName} - {p.StockQuantity} stuks (min: {p.MinimumStock})")
                .ToListAsync();
            
            VoorraadWaarschuwingen = new ObservableCollection<string>(lowStockProducts);
            
            Debug.WriteLine($"Loaded {VoorraadWaarschuwingen.Count} low stock warnings");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading dashboard: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    // Commands
    [RelayCommand]
    private async Task RefreshData()
    {
        try
        {
            IsSyncing = true;
            Debug.WriteLine("MainViewModel: Refreshing data...");
            await LoadDataAsync();
            LaatsteSyncTijd = DateTime.Now.ToString("HH:mm");
            Debug.WriteLine($"MainViewModel: Refresh completed at {LaatsteSyncTijd}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing data: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", $"Kan gegevens niet vernieuwen: {ex.Message}", "OK");
        }
        finally
        {
            IsSyncing = false;
        }
    }
    
    [RelayCommand]
    private async Task NieuwProduct()
    {
        try
        {
            Debug.WriteLine("MainViewModel: Navigating to new product page");
            await Shell.Current.GoToAsync(nameof(Pages.ProductDetailPage));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to product detail: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", "Kan niet navigeren naar product pagina", "OK");
        }
    }
    
    [RelayCommand]
    private async Task NieuweLevering()
    {
        try
        {
            Debug.WriteLine("MainViewModel: Navigating to new delivery page");
            await Shell.Current.GoToAsync(nameof(Pages.DeliveryDetailPage));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to delivery detail: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", "Kan niet navigeren naar levering pagina", "OK");
        }
    }
    
    [RelayCommand]
    private async Task NieuweKlant()
    {
        try
        {
            Debug.WriteLine("MainViewModel: Navigating to new customer page");
            await Shell.Current.GoToAsync(nameof(Pages.CustomerDetailPage));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to customer detail: {ex.Message}");
            await Shell.Current.DisplayAlert("Fout", "Kan niet navigeren naar klant pagina", "OK");
        }
    }
    
    [RelayCommand]
    private async Task OpenRapporten()
    {
        try
        {
            Debug.WriteLine("MainViewModel: Reports feature not yet implemented");
            await Shell.Current.DisplayAlert("Info", "Rapporten functie komt binnenkort beschikbaar", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OpenRapporten: {ex.Message}");
        }
    }
}
