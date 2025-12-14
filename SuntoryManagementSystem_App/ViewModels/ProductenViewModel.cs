using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_App.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.ViewModels;

public partial class ProductenViewModel : ObservableObject
{
    private readonly LocalDbContext _context;
    
    [ObservableProperty]
    private ObservableCollection<Product> producten = new();
    
    [ObservableProperty]
    private ObservableCollection<Product> gefilterdProducten = new();
    
    [ObservableProperty]
    private string zoekTekst = string.Empty;
    
    [ObservableProperty]
    private bool isLoading = false;
    
    // Constructor met DI
    public ProductenViewModel(LocalDbContext context)
    {
        _context = context;
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
            
            foreach (var product in producten)
            {
                Debug.WriteLine($"  - {product.ProductName} (SKU: {product.SKU}, Stock: {product.StockQuantity}, Supplier: {product.Supplier?.SupplierName ?? "NULL"})");
            }
            
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
        await Shell.Current.DisplayAlert("Info", "Nieuw Product - Coming soon!", "OK");
    }
    
    [RelayCommand]
    private async Task BewerkProduct(Product product)
    {
        if (product == null) return;
        Debug.WriteLine($"BewerkProduct: {product.ProductName}");
        await Shell.Current.DisplayAlert("Bewerken", $"Bewerk: {product.ProductName}", "OK");
    }
    
    [RelayCommand]
    private async Task VerwijderProduct(Product product)
    {
        if (product == null) return;
        
        bool bevestiging = await Shell.Current.DisplayAlert(
            "Verwijderen", 
            $"Weet je zeker dat je '{product.ProductName}' wilt verwijderen?",
            "Ja", 
            "Nee"
        );
        
        if (bevestiging)
        {
            try
            {
                product.IsDeleted = true;
                product.DeletedDate = DateTime.Now;
                
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                
                Producten.Remove(product);
                FilterProducten();
                
                await Shell.Current.DisplayAlert("Succes", "Product verwijderd!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Fout bij verwijderen: {ex.Message}", "OK");
            }
        }
    }
    
    [RelayCommand]
    private async Task VoorraadAanpassen(Product product)
    {
        if (product == null) return;
        await Shell.Current.DisplayAlert("Voorraad", $"{product.ProductName}\nHuidige voorraad: {product.StockQuantity}", "OK");
    }
    
    [RelayCommand]
    private async Task Refresh()
    {
        await LoadProductenAsync();
    }
}
