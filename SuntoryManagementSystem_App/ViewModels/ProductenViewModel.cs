using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_App.Data;
using SuntoryManagementSystem_App.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.ViewModels;

public partial class ProductenViewModel : ObservableObject
{
    private readonly LocalDbContext _context;
    private readonly DataService _dataService;
    
    [ObservableProperty]
    private ObservableCollection<Product> producten = new();
    
    [ObservableProperty]
    private ObservableCollection<Product> gefilterdProducten = new();
    
    [ObservableProperty]
    private string zoekTekst = string.Empty;
    
    [ObservableProperty]
    private bool isLoading = false;
    
    // Constructor met DI
    public ProductenViewModel(LocalDbContext context, DataService dataService)
    {
        _context = context;
        _dataService = dataService;
        _ = LoadProductenAsync();
    }
    
    partial void OnZoekTekstChanged(string value)
    {
        FilterProducten();
    }
    
    private void FilterProducten()
    {
        if (string.IsNullOrWhiteSpace(ZoekTekst))
        {
            GefilterdProducten = new ObservableCollection<Product>(Producten);
        }
        else
        {
            var filtered = Producten.Where(p => 
                p.ProductName.Contains(ZoekTekst, StringComparison.OrdinalIgnoreCase) ||
                p.SKU.Contains(ZoekTekst, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(ZoekTekst, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(ZoekTekst, StringComparison.OrdinalIgnoreCase))
            ).ToList();
            
            GefilterdProducten = new ObservableCollection<Product>(filtered);
        }
        
        Debug.WriteLine($"FilterProducten: {GefilterdProducten.Count} producten na filter");
    }
    
    private async Task LoadProductenAsync()
    {
        IsLoading = true;
        
        try
        {
            Debug.WriteLine("LoadProductenAsync: Start loading...");
            
            // Haal NIET-verwijderde producten op, inclusief Supplier
            var producten = await _context.Products
                .Include(p => p.Supplier)
                .Where(p => p.IsDeleted == false)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
            
            Debug.WriteLine($"LoadProductenAsync: Loaded {producten.Count} products from database");
            
            Producten = new ObservableCollection<Product>(producten);
            FilterProducten();
            
            Debug.WriteLine($"LoadProductenAsync: Producten collection has {Producten.Count} items");
            Debug.WriteLine($"LoadProductenAsync: GefilterdProducten collection has {GefilterdProducten.Count} items");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadProductenAsync ERROR: {ex.Message}");
            Debug.WriteLine($"LoadProductenAsync STACK: {ex.StackTrace}");
            
            await Shell.Current.DisplayAlert("Error", $"Kan producten niet laden: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
            Debug.WriteLine("LoadProductenAsync: Finished");
        }
    }
    
    [RelayCommand]
    private async Task VoegProductToe()
    {
        try
        {
            Debug.WriteLine("VoegProductToe: Command triggered");
            Debug.WriteLine("VoegProductToe: Navigating to ProductDetailPage");
            await Shell.Current.GoToAsync(nameof(Pages.ProductDetailPage));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"VoegProductToe ERROR: {ex.Message}");
            Debug.WriteLine($"VoegProductToe STACK: {ex.StackTrace}");
            
            await Shell.Current.DisplayAlert("Error", $"Kan niet navigeren: {ex.Message}", "OK");
        }
    }
    
    [RelayCommand]
    private async Task BekijkProduct(Product? product)
    {
        if (product == null)
        {
            Debug.WriteLine("BekijkProduct: Product is null");
            return;
        }
        
        try
        {
            Debug.WriteLine($"BekijkProduct: Command triggered for {product.ProductName}");
            Debug.WriteLine($"BekijkProduct: Navigating with ProductId={product.ProductId} in VIEW mode (read-only)");
            await Shell.Current.GoToAsync($"{nameof(Pages.ProductDetailPage)}?ProductId={product.ProductId}&ViewMode=true");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"BekijkProduct ERROR: {ex.Message}");
            Debug.WriteLine($"BekijkProduct STACK: {ex.StackTrace}");
            
            await Shell.Current.DisplayAlert("Error", $"Kan niet navigeren: {ex.Message}", "OK");
        }
    }
    
    [RelayCommand]
    private async Task BewerkProduct(Product? product)
    {
        if (product == null)
        {
            Debug.WriteLine("BewerkProduct: Product is null");
            return;
        }
        
        try
        {
            Debug.WriteLine($"BewerkProduct: Command triggered for {product.ProductName}");
            Debug.WriteLine($"BewerkProduct: Navigating with ProductId={product.ProductId} in EDIT mode");
            await Shell.Current.GoToAsync($"{nameof(Pages.ProductDetailPage)}?ProductId={product.ProductId}&ViewMode=false");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"BewerkProduct ERROR: {ex.Message}");
            Debug.WriteLine($"BewerkProduct STACK: {ex.StackTrace}");
            
            await Shell.Current.DisplayAlert("Error", $"Kan niet navigeren: {ex.Message}", "OK");
        }
    }
    
    [RelayCommand]
    private async Task VerwijderProduct(Product? product)
    {
        if (product == null)
        {
            Debug.WriteLine("VerwijderProduct: Product is null");
            return;
        }
        
        try
        {
            Debug.WriteLine($"VerwijderProduct: Command triggered for {product.ProductName}");
            
            bool bevestiging = await Shell.Current.DisplayAlert(
                "Verwijderen", 
                $"Weet je zeker dat je '{product.ProductName}' wilt verwijderen?",
                "Ja",
                "Nee");
            
            if (bevestiging)
            {
                // Gebruik DataService voor realtime sync met server
                await _dataService.DeleteProductAsync(product);
                
                Producten.Remove(product);
                FilterProducten();
                
                await Shell.Current.DisplayAlert("Succes", "Product verwijderd!", "OK");
                
                Debug.WriteLine($"VerwijderProduct: Product {product.ProductName} deleted (local + server if online)");
            }
            else
            {
                Debug.WriteLine($"VerwijderProduct: User cancelled deletion");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"VerwijderProduct ERROR: {ex.Message}");
            Debug.WriteLine($"VerwijderProduct STACK: {ex.StackTrace}");
            
            await Shell.Current.DisplayAlert("Error", $"Fout bij verwijderen: {ex.Message}", "OK");
        }
    }
    
    [RelayCommand]
    private async Task Refresh()
    {
        Debug.WriteLine("Refresh: Command triggered");
        await LoadProductenAsync();
    }
}
