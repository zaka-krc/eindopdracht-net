using SuntoryManagementSystem_App.Data;
using SuntoryManagementSystem_App.Services;
using SuntoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.Pages;

[QueryProperty(nameof(ProductId), nameof(ProductId))]
[QueryProperty(nameof(StockAdjustmentMode), nameof(StockAdjustmentMode))]
[QueryProperty(nameof(ViewMode), nameof(ViewMode))]
public partial class ProductDetailPage : ContentPage
{
    private readonly LocalDbContext _context;
    private readonly DataService _dataService;
    private Product? _product;
    private string? _productIdString;
    private bool _stockAdjustmentMode;
    private bool _viewMode;
    private int _originalStockQuantity;
    private bool _isLoaded = false;
    
    // Lijsten voor pickers
    private List<Supplier> _suppliers = new();
    
    public string? ProductId
    {
        get => _productIdString;
        set
        {
            _productIdString = value;
            Debug.WriteLine($"ProductDetailPage: ProductId set to {value}");
            
            // Load data only if not already loaded
            if (!_isLoaded)
            {
                _isLoaded = true;
                _ = LoadDataAsync();
            }
        }
    }
    
    public string? StockAdjustmentMode
    {
        get => _stockAdjustmentMode.ToString();
        set
        {
            _stockAdjustmentMode = value?.ToLower() == "true";
            Debug.WriteLine($"ProductDetailPage: StockAdjustmentMode set to {_stockAdjustmentMode}");
        }
    }
    
    public string? ViewMode
    {
        get => _viewMode.ToString();
        set
        {
            _viewMode = value?.ToLower() == "true";
            Debug.WriteLine($"ProductDetailPage: ViewMode set to {_viewMode}");
        }
    }
    
    public ProductDetailPage(LocalDbContext context, DataService dataService)
    {
        InitializeComponent();
        _context = context;
        _dataService = dataService;
    }
    
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        // If ProductId wasn't set via QueryProperty (new product), load now
        if (!_isLoaded)
        {
            _isLoaded = true;
            _ = LoadDataAsync();
        }
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            Debug.WriteLine($"LoadDataAsync: Starting... ProductId={_productIdString}");
            
            // Load suppliers
            _suppliers = await _context.Suppliers
                .Where(s => !s.IsDeleted && s.Status == "Active")
                .OrderBy(s => s.SupplierName)
                .ToListAsync();
            
            Debug.WriteLine($"Loaded: {_suppliers.Count} suppliers");
            
            // Bind supplier picker
            SupplierPicker.ItemsSource = _suppliers;
            
            // Try parse ProductId
            int? productId = null;
            if (!string.IsNullOrEmpty(_productIdString) && int.TryParse(_productIdString, out int parsedId))
            {
                productId = parsedId;
            }
            
            if (productId.HasValue && productId.Value > 0)
            {
                Debug.WriteLine($"LoadDataAsync: Edit mode - loading product {productId}");
                // Edit mode - load existing product
                _product = await _context.Products
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.ProductId == productId.Value);
                
                if (_product != null)
                {
                    Debug.WriteLine($"LoadDataAsync: Found product {_product.ProductName}");
                    _originalStockQuantity = _product.StockQuantity;
                    BindProductToForm();
                    
                    if (_viewMode)
                    {
                        Title = "Product Details";
                        DisableAllFields();
                        SaveButton.IsVisible = false;
                    }
                    else if (_stockAdjustmentMode)
                    {
                        Title = "Voorraad Aanpassen";
                        // Disable all fields except stock quantity
                        ProductNameEntry.IsEnabled = false;
                        SKUEntry.IsEnabled = false;
                        CategoryPicker.IsEnabled = false;
                        SupplierPicker.IsEnabled = false;
                        PurchasePriceEntry.IsEnabled = false;
                        SellingPriceEntry.IsEnabled = false;
                        MinimumStockEntry.IsEnabled = false;
                        IsActivePicker.IsEnabled = false;
                        DescriptionEditor.IsEnabled = false;
                    }
                    else
                    {
                        Title = "Product Wijzigen";
                    }
                }
                else
                {
                    Debug.WriteLine($"LoadDataAsync: Product {productId} not found");
                    await DisplayAlert("Fout", "Product niet gevonden", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            else
            {
                Debug.WriteLine("LoadDataAsync: Create mode - new product");
                Title = "Nieuw Product";
                // Create mode - new product
                _product = new Product
                {
                    IsActive = true,
                    StockQuantity = 0,
                    MinimumStock = 10,
                    PurchasePrice = 0,
                    SellingPrice = 0,
                    CreatedDate = DateTime.Now,
                    Category = "Frisdranken",
                    Description = string.Empty,
                    ProductName = string.Empty,
                    SKU = string.Empty
                };
                
                _originalStockQuantity = 0;
                
                // Set defaults
                CategoryPicker.SelectedIndex = 0; // Frisdranken
                IsActivePicker.SelectedIndex = 0; // Actief
                StockQuantityEntry.Text = "0";
                MinimumStockEntry.Text = "10";
                PurchasePriceEntry.Text = "0.00";
                SellingPriceEntry.Text = "0.00";
                
                if (_suppliers.Any())
                {
                    SupplierPicker.SelectedIndex = 0;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadDataAsync ERROR: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"Kan gegevens niet laden: {ex.Message}", "OK");
        }
    }
    
    private void DisableAllFields()
    {
        ProductNameEntry.IsEnabled = false;
        SKUEntry.IsEnabled = false;
        CategoryPicker.IsEnabled = false;
        SupplierPicker.IsEnabled = false;
        PurchasePriceEntry.IsEnabled = false;
        SellingPriceEntry.IsEnabled = false;
        StockQuantityEntry.IsEnabled = false;
        MinimumStockEntry.IsEnabled = false;
        IsActivePicker.IsEnabled = false;
        DescriptionEditor.IsEnabled = false;
    }
    
    private void BindProductToForm()
    {
        if (_product == null) return;
        
        Debug.WriteLine($"BindProductToForm: Binding {_product.ProductName}");
        
        ProductNameEntry.Text = _product.ProductName;
        SKUEntry.Text = _product.SKU;
        CategoryPicker.SelectedItem = _product.Category;
        
        // Bind supplier
        var supplier = _suppliers.FirstOrDefault(s => s.SupplierId == _product.SupplierId);
        if (supplier != null)
        {
            SupplierPicker.SelectedItem = supplier;
        }
        
        PurchasePriceEntry.Text = _product.PurchasePrice.ToString("F2");
        SellingPriceEntry.Text = _product.SellingPrice.ToString("F2");
        StockQuantityEntry.Text = _product.StockQuantity.ToString();
        MinimumStockEntry.Text = _product.MinimumStock.ToString();
        IsActivePicker.SelectedIndex = _product.IsActive ? 0 : 1;
        DescriptionEditor.Text = _product.Description ?? string.Empty;
    }
    
    private void OnStockQuantityChanged(object sender, TextChangedEventArgs e)
    {
        // Only show warning in edit mode
        if (_product == null || string.IsNullOrEmpty(_productIdString)) return;
        
        if (int.TryParse(e.NewTextValue, out int newStock))
        {
            int change = newStock - _originalStockQuantity;
            
            if (change != 0)
            {
                StockChangeWarning.IsVisible = true;
                string changeSymbol = change > 0 ? "+" : "";
                StockChangeLabel.Text = $"Voorraad wijzigt: {_originalStockQuantity} ? {newStock} ({changeSymbol}{change})";
                
                // Show appropriate adjustment type picker
                if (change < 0)
                {
                    AdjustmentTypeContainer.IsVisible = true;
                    AdditionTypeContainer.IsVisible = false;
                    AdjustmentTypePicker.SelectedIndex = 0; // Default to Removal
                }
                else
                {
                    AdjustmentTypeContainer.IsVisible = false;
                    AdditionTypeContainer.IsVisible = true;
                    AdditionTypePicker.SelectedIndex = 0; // Default to Addition
                }
            }
            else
            {
                StockChangeWarning.IsVisible = false;
                AdjustmentTypeContainer.IsVisible = false;
                AdditionTypeContainer.IsVisible = false;
            }
        }
        else
        {
            StockChangeWarning.IsVisible = false;
            AdjustmentTypeContainer.IsVisible = false;
            AdditionTypeContainer.IsVisible = false;
        }
    }
    
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Validatie
            if (string.IsNullOrWhiteSpace(ProductNameEntry.Text))
            {
                await DisplayAlert("Fout", "Productnaam is verplicht", "OK");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(SKUEntry.Text))
            {
                await DisplayAlert("Fout", "SKU is verplicht", "OK");
                return;
            }
            
            if (SupplierPicker.SelectedItem == null)
            {
                await DisplayAlert("Fout", "Selecteer een leverancier", "OK");
                return;
            }
            
            if (!decimal.TryParse(PurchasePriceEntry.Text, out decimal purchasePrice) || purchasePrice < 0)
            {
                await DisplayAlert("Fout", "Voer een geldige inkoopprijs in", "OK");
                return;
            }
            
            if (!decimal.TryParse(SellingPriceEntry.Text, out decimal sellingPrice) || sellingPrice < 0)
            {
                await DisplayAlert("Fout", "Voer een geldige verkoopprijs in", "OK");
                return;
            }
            
            if (!int.TryParse(StockQuantityEntry.Text, out int stockQuantity) || stockQuantity < 0)
            {
                await DisplayAlert("Fout", "Voer een geldige voorraad in", "OK");
                return;
            }
            
            if (!int.TryParse(MinimumStockEntry.Text, out int minimumStock) || minimumStock < 0)
            {
                await DisplayAlert("Fout", "Voer een geldige minimum voorraad in", "OK");
                return;
            }
            
            // Check for stock adjustment
            bool isNewProduct = string.IsNullOrEmpty(_productIdString);
            bool stockChanged = !string.IsNullOrEmpty(_productIdString) && stockQuantity != _originalStockQuantity;
            string adjustmentType = "Addition";
            
            if (stockChanged)
            {
                int quantityChange = stockQuantity - _originalStockQuantity;
                
                if (quantityChange < 0 && AdjustmentTypePicker.SelectedItem == null)
                {
                    await DisplayAlert("Fout", "Selecteer een reden voor voorraad vermindering", "OK");
                    return;
                }
                
                if (quantityChange > 0 && AdditionTypePicker.SelectedItem == null)
                {
                    await DisplayAlert("Fout", "Selecteer een reden voor voorraad toevoeging", "OK");
                    return;
                }
                
                // Parse adjustment type from picker
                if (quantityChange < 0)
                {
                    var selectedType = AdjustmentTypePicker.SelectedItem?.ToString();
                    if (!string.IsNullOrEmpty(selectedType))
                    {
                        adjustmentType = selectedType.Split('-')[0].Trim();
                    }
                }
                else
                {
                    var selectedType = AdditionTypePicker.SelectedItem?.ToString();
                    if (!string.IsNullOrEmpty(selectedType))
                    {
                        adjustmentType = selectedType.Split('-')[0].Trim();
                    }
                }
            }
            
            // Update product properties
            if (_product == null) return;
            
            _product.ProductName = ProductNameEntry.Text.Trim();
            _product.SKU = SKUEntry.Text.Trim();
            _product.Category = CategoryPicker.SelectedItem?.ToString() ?? "Frisdranken";
            _product.SupplierId = (SupplierPicker.SelectedItem as Supplier)?.SupplierId ?? 0;
            _product.PurchasePrice = purchasePrice;
            _product.SellingPrice = sellingPrice;
            _product.StockQuantity = stockQuantity;
            _product.MinimumStock = minimumStock;
            _product.IsActive = IsActivePicker.SelectedIndex == 0;
            _product.Description = DescriptionEditor.Text?.Trim() ?? string.Empty;
            
            // Save product
            if (isNewProduct)
            {
                // Gebruik DataService voor realtime sync met server
                var savedProduct = await _dataService.CreateProductAsync(_product);
                _product = savedProduct; // Update met server ID indien online
                
                Debug.WriteLine($"Created product with ID: {_product.ProductId}");
                
                // Create initial stock adjustment if stock > 0
                if (stockQuantity > 0)
                {
                    await CreateStockAdjustment(0, stockQuantity, "Addition", true);
                }
            }
            else
            {
                // Gebruik DataService voor realtime sync met server
                await _dataService.UpdateProductAsync(_product);
                
                Debug.WriteLine($"Updated product {_product.ProductId}");
                
                // Create stock adjustment if stock changed
                if (stockChanged)
                {
                    await CreateStockAdjustment(_originalStockQuantity, stockQuantity, adjustmentType, false);
                }
            }
            
            // Show success message
            var successMessage = BuildSuccessMessage(isNewProduct, stockChanged, _originalStockQuantity, stockQuantity, adjustmentType);
            await DisplayAlert("Succes", successMessage, "OK");
            
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnSaveClicked ERROR: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"Fout bij opslaan: {ex.Message}", "OK");
        }
    }
    
    private async Task CreateStockAdjustment(int previousQuantity, int newQuantity, string adjustmentType, bool isNewProduct)
    {
        if (_product == null) return;
        
        int quantityChange = newQuantity - previousQuantity;
        string reason;
        
        if (isNewProduct)
        {
            reason = $"Nieuw product aangemaakt met initiële voorraad van {newQuantity} stuks";
        }
        else
        {
            if (quantityChange > 0)
            {
                reason = adjustmentType switch
                {
                    "Correction" => $"Voorraad correctie na inventarisatie: +{quantityChange} stuks bijgeteld",
                    _ => $"Handmatige voorraad toevoeging: +{quantityChange} stuks toegevoegd"
                };
            }
            else
            {
                reason = adjustmentType switch
                {
                    "Damage" => $"Voorraad verminderd door schade: {quantityChange} stuks beschadigd",
                    "Theft" => $"Voorraad verminderd door diefstal: {quantityChange} stuks gestolen/vermist",
                    "Correction" => $"Voorraad correctie na inventarisatie: {quantityChange} stuks",
                    _ => $"Handmatige voorraad verwijdering: {quantityChange} stuks verwijderd"
                };
            }
        }
        
        var adjustment = new StockAdjustment
        {
            ProductId = _product.ProductId,
            AdjustmentType = adjustmentType,
            QuantityChange = quantityChange,
            PreviousQuantity = previousQuantity,
            NewQuantity = newQuantity,
            Reason = reason,
            AdjustedBy = "App User",
            AdjustmentDate = DateTime.Now
        };
        
        await _context.StockAdjustments.AddAsync(adjustment);
        await _context.SaveChangesAsync();
        
        Debug.WriteLine($"Created stock adjustment: {adjustment.Reason}");
        
        // Create or update stock alerts if needed
        if (newQuantity < _product.MinimumStock)
        {
            await CreateOrUpdateStockAlert(newQuantity);
        }
        else if (previousQuantity < _product.MinimumStock && newQuantity >= _product.MinimumStock)
        {
            await ResolveStockAlerts();
        }
    }
    
    private async Task CreateOrUpdateStockAlert(int currentStock)
    {
        if (_product == null) return;
        
        var existingAlert = await _context.StockAlerts
            .FirstOrDefaultAsync(sa => sa.ProductId == _product.ProductId 
                && sa.Status == "Active" 
                && !sa.IsDeleted);
        
        if (existingAlert == null)
        {
            string alertType = currentStock == 0 ? "Out of Stock" : 
                             currentStock < (_product.MinimumStock / 2) ? "Critical" : 
                             "Low Stock";
            
            var alert = new StockAlert
            {
                ProductId = _product.ProductId,
                AlertType = alertType,
                Status = "Active",
                CreatedDate = DateTime.Now,
                Notes = $"Voorraad is {currentStock} stuks, minimum is {_product.MinimumStock}"
            };
            
            await _context.StockAlerts.AddAsync(alert);
            await _context.SaveChangesAsync();
            
            Debug.WriteLine($"Created stock alert: {alertType}");
        }
        else
        {
            existingAlert.AlertType = currentStock == 0 ? "Out of Stock" : 
                                    currentStock < (_product.MinimumStock / 2) ? "Critical" : 
                                    "Low Stock";
            existingAlert.Notes = $"Voorraad is {currentStock} stuks, minimum is {_product.MinimumStock}";
            
            _context.StockAlerts.Update(existingAlert);
            await _context.SaveChangesAsync();
            
            Debug.WriteLine($"Updated stock alert: {existingAlert.AlertType}");
        }
    }
    
    private async Task ResolveStockAlerts()
    {
        if (_product == null) return;
        
        var activeAlerts = await _context.StockAlerts
            .Where(sa => sa.ProductId == _product.ProductId 
                && sa.Status == "Active" 
                && !sa.IsDeleted)
            .ToListAsync();
        
        foreach (var alert in activeAlerts)
        {
            alert.Status = "Resolved";
            alert.ResolvedDate = DateTime.Now;
            alert.Notes += " - Opgelost: voorraad weer boven minimum";
            
            _context.StockAlerts.Update(alert);
        }
        
        await _context.SaveChangesAsync();
        
        Debug.WriteLine($"Resolved {activeAlerts.Count} stock alerts");
    }
    
    private string BuildSuccessMessage(bool isNewProduct, bool stockChanged, int previousQty, int newQty, string adjustmentType)
    {
        if (_product == null) return "Product opgeslagen!";
        
        var message = new System.Text.StringBuilder();
        
        if (isNewProduct)
        {
            message.AppendLine("? NIEUW PRODUCT AANGEMAAKT");
            message.AppendLine();
            message.AppendLine($"Product: {_product.ProductName}");
            message.AppendLine($"SKU: {_product.SKU}");
            
            if (newQty > 0)
            {
                message.AppendLine();
                message.AppendLine($"Initiële voorraad: {newQty} stuks");
                message.AppendLine($"Minimum: {_product.MinimumStock} stuks");
                
                if (newQty < _product.MinimumStock)
                {
                    message.AppendLine();
                    message.AppendLine("?? WAARSCHUWING:");
                    message.AppendLine($"Voorraad is onder minimum!");
                }
            }
        }
        else if (stockChanged)
        {
            message.AppendLine("? VOORRAAD AANPASSING");
            message.AppendLine();
            message.AppendLine($"Product: {_product.ProductName}");
            message.AppendLine();
            
            int quantityChange = newQty - previousQty;
            string changeSymbol = quantityChange > 0 ? "+" : "";
            
            message.AppendLine($"Vorige voorraad: {previousQty} stuks");
            message.AppendLine($"Wijziging: {changeSymbol}{quantityChange} stuks");
            message.AppendLine($"Nieuwe voorraad: {newQty} stuks");
            message.AppendLine($"Type: {adjustmentType}");
            
            if (newQty < _product.MinimumStock)
            {
                message.AppendLine();
                message.AppendLine("?? WAARSCHUWING:");
                message.AppendLine($"Voorraad is onder minimum ({_product.MinimumStock})!");
            }
            else if (previousQty < _product.MinimumStock && newQty >= _product.MinimumStock)
            {
                message.AppendLine();
                message.AppendLine("? VOORRAAD HERSTELD:");
                message.AppendLine("Voorraad is weer boven minimum!");
            }
        }
        else
        {
            message.AppendLine("? Product succesvol opgeslagen!");
        }
        
        return message.ToString();
    }
    
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
