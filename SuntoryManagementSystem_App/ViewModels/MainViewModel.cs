using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SuntoryManagementSystem_App.Data;
using Microsoft.EntityFrameworkCore;

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
            // Haal ECHTE stats uit database
            AantalProducten = await _context.Products.CountAsync(p => !p.IsDeleted);
            LageVoorraadCount = await _context.StockAlerts.CountAsync(a => a.Status == "Active");
            OpenstaandeLeveringen = await _context.Deliveries.CountAsync(d => !d.IsDeleted && d.Status == "Gepland");
            ActieveKlanten = await _context.Customers.CountAsync(c => c.Status == "Active");
            
            // Haal low stock producten op
            var lowStockProducts = await _context.Products
                .Where(p => !p.IsDeleted && p.StockQuantity < p.MinimumStock)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .Select(p => $"{p.ProductName} - {p.StockQuantity} stuks resterend")
                .ToListAsync();
            
            VoorraadWaarschuwingen = new ObservableCollection<string>(lowStockProducts);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
        }
    }
    
    // Commands
    [RelayCommand]
    private async Task RefreshData()
    {
        IsSyncing = true;
        await LoadDataAsync();
        LaatsteSyncTijd = DateTime.Now.ToString("HH:mm");
        IsSyncing = false;
    }
    
    [RelayCommand]
    private async Task NieuwProduct()
    {
        // TODO: Navigeer naar ProductDetailPage
        await Shell.Current.DisplayAlert("Info", "Nieuw Product - Coming soon!", "OK");
    }
    
    [RelayCommand]
    private async Task OpenRapporten()
    {
        // TODO: Navigeer naar RapportenPage
        await Shell.Current.DisplayAlert("Info", "Rapporten - Coming soon!", "OK");
    }
}
