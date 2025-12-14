using SuntoryManagementSystem_App.Data;
using SuntoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.Pages;

[QueryProperty(nameof(DeliveryId), nameof(DeliveryId))]
[QueryProperty(nameof(ViewMode), nameof(ViewMode))]
public partial class DeliveryDetailPage : ContentPage
{
    private readonly LocalDbContext _context;
    private Delivery? _delivery;
    private string? _deliveryIdString;
    private bool _viewMode;
    private bool _isLoaded = false;
    
    // Lijsten voor pickers
    private List<Supplier> _suppliers = new();
    private List<Customer> _customers = new();
    private List<Vehicle> _vehicles = new();
    private List<Product> _products = new();
    
    // Delivery items voor deze levering
    private ObservableCollection<DeliveryItemViewModel> _deliveryItems = new();
    
    public string? DeliveryId
    {
        get => _deliveryIdString;
        set
        {
            _deliveryIdString = value;
            Debug.WriteLine($"DeliveryDetailPage: DeliveryId set to {value}");
            
            // Load data only if not already loaded
            if (!_isLoaded)
            {
                _isLoaded = true;
                _ = LoadDataAsync();
            }
        }
    }
    
    public string? ViewMode
    {
        get => _viewMode.ToString();
        set
        {
            _viewMode = value?.ToLower() == "true";
            Debug.WriteLine($"DeliveryDetailPage: ViewMode set to {_viewMode}");
        }
    }

    public DeliveryDetailPage(LocalDbContext context)
    {
        InitializeComponent();
        _context = context;
    }
    
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        // If DeliveryId wasn't set via QueryProperty (new delivery), load now
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
            Debug.WriteLine($"LoadDataAsync: Starting... DeliveryId={_deliveryIdString}, ViewMode={_viewMode}");
            
            // Load referentie data
            _suppliers = await _context.Suppliers
                .Where(s => !s.IsDeleted && s.Status == "Active")
                .OrderBy(s => s.SupplierName)
                .ToListAsync();
            
            _customers = await _context.Customers
                .Where(c => !c.IsDeleted && c.Status == "Active")
                .OrderBy(c => c.CustomerName)
                .ToListAsync();
            
            _vehicles = await _context.Vehicles
                .Where(v => !v.IsDeleted && v.IsAvailable)
                .OrderBy(v => v.LicensePlate)
                .ToListAsync();
            
            _products = await _context.Products
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
            
            Debug.WriteLine($"Loaded: {_suppliers.Count} suppliers, {_customers.Count} customers, {_vehicles.Count} vehicles, {_products.Count} products");
            
            // Bind pickers
            SupplierPicker.ItemsSource = _suppliers;
            CustomerPicker.ItemsSource = _customers;
            VehiclePicker.ItemsSource = _vehicles;
            
            // Try parse DeliveryId
            int? deliveryId = null;
            if (!string.IsNullOrEmpty(_deliveryIdString) && int.TryParse(_deliveryIdString, out int parsedId))
            {
                deliveryId = parsedId;
            }
            
            if (deliveryId.HasValue && deliveryId.Value > 0)
            {
                Debug.WriteLine($"LoadDataAsync: Loading delivery {deliveryId}");
                // Edit/View mode - load existing delivery
                _delivery = await _context.Deliveries
                    .Include(d => d.Supplier)
                    .Include(d => d.Customer)
                    .Include(d => d.Vehicle)
                    .Include(d => d.DeliveryItems)
                        .ThenInclude(di => di.Product)
                    .FirstOrDefaultAsync(d => d.DeliveryId == deliveryId.Value);
                
                if (_delivery != null)
                {
                    Debug.WriteLine($"LoadDataAsync: Found delivery {_delivery.ReferenceNumber}");
                    BindDeliveryToForm();
                    
                    if (_viewMode)
                    {
                        Title = "Levering Bekijken";
                        DisableEditing();
                    }
                    else if (_delivery.IsProcessed || _delivery.Status == "Geannuleerd")
                    {
                        Title = "Levering Bekijken";
                        DisableEditing();
                    }
                    else
                    {
                        Title = "Levering Wijzigen";
                    }
                }
                else
                {
                    Debug.WriteLine($"LoadDataAsync: Delivery {deliveryId} not found");
                    await DisplayAlert("Fout", "Levering niet gevonden", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            else
            {
                Debug.WriteLine("LoadDataAsync: Create mode - new delivery");
                Title = "Nieuwe Levering";
                // Create mode - nieuwe delivery
                _delivery = new Delivery
                {
                    DeliveryType = "Incoming",
                    Status = "Gepland",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(1),
                    CreatedDate = DateTime.Now,
                    ReferenceNumber = GenerateReferenceNumber("Incoming"),
                    Notes = string.Empty
                };
                
                TypePicker.SelectedIndex = 0; // Incoming
                StatusPicker.SelectedIndex = 0; // Gepland
                ReferenceNumberEntry.Text = _delivery.ReferenceNumber;
                ExpectedDatePicker.Date = _delivery.ExpectedDeliveryDate;
                
                if (_suppliers.Any())
                {
                    SupplierPicker.SelectedIndex = 0;
                }
                
                UpdatePartnerVisibility();
            }
            
            DeliveryItemsCollection.ItemsSource = _deliveryItems;
            UpdateTotalAmount();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadDataAsync ERROR: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"Kan gegevens niet laden: {ex.Message}", "OK");
        }
    }
    
    private void BindDeliveryToForm()
    {
        if (_delivery == null) return;
        
        Debug.WriteLine($"BindDeliveryToForm: Binding {_delivery.ReferenceNumber}");
        
        // Bind type
        TypePicker.SelectedItem = _delivery.DeliveryType;
        
        // Bind reference number
        ReferenceNumberEntry.Text = _delivery.ReferenceNumber;
        
        // Bind supplier/customer
        if (_delivery.DeliveryType == "Incoming" && _delivery.SupplierId.HasValue)
        {
            var supplier = _suppliers.FirstOrDefault(s => s.SupplierId == _delivery.SupplierId.Value);
            if (supplier != null)
            {
                SupplierPicker.SelectedItem = supplier;
            }
        }
        else if (_delivery.DeliveryType == "Outgoing" && _delivery.CustomerId.HasValue)
        {
            var customer = _customers.FirstOrDefault(c => c.CustomerId == _delivery.CustomerId.Value);
            if (customer != null)
            {
                CustomerPicker.SelectedItem = customer;
            }
        }
        
        // Bind vehicle
        if (_delivery.VehicleId.HasValue)
        {
            var vehicle = _vehicles.FirstOrDefault(v => v.VehicleId == _delivery.VehicleId.Value);
            if (vehicle != null)
            {
                VehiclePicker.SelectedItem = vehicle;
            }
        }
        
        // Bind dates
        ExpectedDatePicker.Date = _delivery.ExpectedDeliveryDate;
        
        // Bind status
        StatusPicker.SelectedItem = _delivery.Status;
        
        // Bind notes
        NotesEditor.Text = _delivery.Notes ?? string.Empty;
        
        // Bind delivery items
        _deliveryItems.Clear();
        if (_delivery.DeliveryItems != null && _delivery.DeliveryItems.Any())
        {
            foreach (var item in _delivery.DeliveryItems.Where(di => !di.IsDeleted))
            {
                _deliveryItems.Add(new DeliveryItemViewModel
                {
                    DeliveryItemId = item.DeliveryItemId,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.ProductName ?? "Onbekend product",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SubTotal = item.Total
                });
            }
        }
        
        UpdatePartnerVisibility();
        
        // Disable editing if processed or cancelled
        if (_delivery.IsProcessed || _delivery.Status == "Geannuleerd")
        {
            DisableEditing();
        }
    }
    
    private void DisableEditing()
    {
        TypePicker.IsEnabled = false;
        ReferenceNumberEntry.IsReadOnly = true;
        SupplierPicker.IsEnabled = false;
        CustomerPicker.IsEnabled = false;
        VehiclePicker.IsEnabled = false;
        ExpectedDatePicker.IsEnabled = false;
        StatusPicker.IsEnabled = false;
        NotesEditor.IsReadOnly = true;
        AddItemButton.IsEnabled = false;
        SaveButton.IsEnabled = false;
    }
    
    private void OnTypePickerChanged(object sender, EventArgs e)
    {
        UpdatePartnerVisibility();
        
        // Generate new reference number when type changes (only for new deliveries)
        int? deliveryId = null;
        if (!string.IsNullOrEmpty(_deliveryIdString) && int.TryParse(_deliveryIdString, out int parsedId))
        {
            deliveryId = parsedId;
        }
        
        if (_delivery != null && (!deliveryId.HasValue || deliveryId == 0))
        {
            var selectedType = TypePicker.SelectedItem?.ToString() ?? "Incoming";
            ReferenceNumberEntry.Text = GenerateReferenceNumber(selectedType);
        }
    }
    
    private void UpdatePartnerVisibility()
    {
        var selectedType = TypePicker.SelectedItem?.ToString() ?? "Incoming";
        
        if (selectedType == "Incoming")
        {
            SupplierLabel.IsVisible = true;
            SupplierPicker.IsVisible = true;
            CustomerLabel.IsVisible = false;
            CustomerPicker.IsVisible = false;
        }
        else
        {
            SupplierLabel.IsVisible = false;
            SupplierPicker.IsVisible = false;
            CustomerLabel.IsVisible = true;
            CustomerPicker.IsVisible = true;
        }
    }
    
    private string GenerateReferenceNumber(string type)
    {
        var prefix = type == "Incoming" ? "INC" : "OUT";
        var year = DateTime.Now.Year;
        var random = new Random().Next(1000, 9999);
        return $"{prefix}-{year}-{random}";
    }
    
    private async void OnAddItemClicked(object sender, EventArgs e)
    {
        try
        {
            // Show product selection dialog
            var productNames = _products.Select(p => p.ProductName).ToArray();
            
            if (!productNames.Any())
            {
                await DisplayAlert("Geen producten", "Er zijn geen actieve producten beschikbaar", "OK");
                return;
            }
            
            var selectedProduct = await DisplayActionSheet("Selecteer Product", "Annuleren", null, productNames);
            
            if (selectedProduct == "Annuleren" || string.IsNullOrEmpty(selectedProduct))
                return;
            
            var product = _products.FirstOrDefault(p => p.ProductName == selectedProduct);
            if (product == null) return;
            
            // Check delivery type for appropriate price
            var deliveryType = TypePicker.SelectedItem?.ToString() ?? "Incoming";
            decimal defaultPrice = deliveryType == "Incoming" ? product.PurchasePrice : product.SellingPrice;
            
            // Check for outgoing deliveries - validate stock
            if (deliveryType == "Outgoing")
            {
                int alreadyInList = _deliveryItems
                    .Where(i => i.ProductId == product.ProductId)
                    .Sum(i => i.Quantity);
                
                if (alreadyInList >= product.StockQuantity)
                {
                    await DisplayAlert("Onvoldoende voorraad", 
                        $"Product: {product.ProductName}\n" +
                        $"Beschikbare voorraad: {product.StockQuantity}\n" +
                        $"Al in levering: {alreadyInList}\n\n" +
                        "Je kunt geen extra items meer toevoegen.", 
                        "OK");
                    return;
                }
            }
            
            // Ask for quantity
            var quantityStr = await DisplayPromptAsync("Hoeveelheid", 
                $"Hoeveel {product.ProductName}?" + 
                (deliveryType == "Outgoing" ? $"\n\nBeschikbaar: {product.StockQuantity}" : ""), 
                "OK", "Annuleren", "1", keyboard: Keyboard.Numeric);
            
            if (string.IsNullOrEmpty(quantityStr)) return;
            
            if (!int.TryParse(quantityStr, out int quantity) || quantity <= 0)
            {
                await DisplayAlert("Fout", "Voer een geldige hoeveelheid in", "OK");
                return;
            }
            
            // Validate stock for outgoing
            if (deliveryType == "Outgoing")
            {
                int alreadyInList = _deliveryItems
                    .Where(i => i.ProductId == product.ProductId)
                    .Sum(i => i.Quantity);
                int totalRequested = alreadyInList + quantity;
                
                if (totalRequested > product.StockQuantity)
                {
                    await DisplayAlert("Onvoldoende voorraad", 
                        $"Product: {product.ProductName}\n" +
                        $"Beschikbare voorraad: {product.StockQuantity}\n" +
                        $"Al in levering: {alreadyInList}\n" +
                        $"Gevraagd: {quantity}\n" +
                        $"Totaal: {totalRequested}\n\n" +
                        $"Maximaal beschikbaar: {product.StockQuantity - alreadyInList}", 
                        "OK");
                    return;
                }
                
                if (totalRequested == product.StockQuantity)
                {
                    var confirm = await DisplayAlert("Volledige voorraad", 
                        $"Je gebruikt de HELE voorraad van '{product.ProductName}'.\n" +
                        $"Totaal: {totalRequested} van {product.StockQuantity} stuks\n\n" +
                        "Wil je doorgaan?", 
                        "Ja", "Nee");
                    
                    if (!confirm) return;
                }
            }
            
            // Ask for unit price
            var priceStr = await DisplayPromptAsync("Prijs", 
                $"Prijs per eenheid?\n(Standaard: EUR {defaultPrice:F2})", 
                "OK", "Annuleren", defaultPrice.ToString("F2"), keyboard: Keyboard.Numeric);
            
            if (string.IsNullOrEmpty(priceStr)) return;
            
            if (!decimal.TryParse(priceStr, out decimal unitPrice) || unitPrice < 0)
            {
                await DisplayAlert("Fout", "Voer een geldige prijs in", "OK");
                return;
            }
            
            // Add item
            var newItem = new DeliveryItemViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Quantity = quantity,
                UnitPrice = unitPrice,
                SubTotal = quantity * unitPrice
            };
            
            _deliveryItems.Add(newItem);
            UpdateTotalAmount();
            
            Debug.WriteLine($"Added item: {newItem.ProductName} x {newItem.Quantity} @ EUR {newItem.UnitPrice}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnAddItemClicked ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Fout bij toevoegen item: {ex.Message}", "OK");
        }
    }
    
    private void OnRemoveItemClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is DeliveryItemViewModel item)
        {
            _deliveryItems.Remove(item);
            UpdateTotalAmount();
            
            Debug.WriteLine($"Removed item: {item.ProductName}");
        }
    }
    
    private void UpdateTotalAmount()
    {
        var total = _deliveryItems.Sum(i => i.SubTotal);
        TotalAmountLabel.Text = $"EUR {total:F2}";
    }
    
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Validatie
            if (string.IsNullOrWhiteSpace(ReferenceNumberEntry.Text))
            {
                await DisplayAlert("Fout", "Referentienummer is verplicht", "OK");
                return;
            }
            
            var deliveryType = TypePicker.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(deliveryType))
            {
                await DisplayAlert("Fout", "Selecteer een type levering", "OK");
                return;
            }
            
            // Check supplier/customer
            if (deliveryType == "Incoming" && SupplierPicker.SelectedItem == null)
            {
                await DisplayAlert("Fout", "Selecteer een leverancier", "OK");
                return;
            }
            
            if (deliveryType == "Outgoing" && CustomerPicker.SelectedItem == null)
            {
                await DisplayAlert("Fout", "Selecteer een klant", "OK");
                return;
            }
            
            if (!_deliveryItems.Any())
            {
                await DisplayAlert("Fout", "Voeg minimaal een product toe aan de levering", "OK");
                return;
            }
            
            // Parse delivery ID
            int? deliveryId = null;
            if (!string.IsNullOrEmpty(_deliveryIdString) && int.TryParse(_deliveryIdString, out int parsedId))
            {
                deliveryId = parsedId;
            }
            
            // Extra validatie voor outgoing deliveries - check voorraad
            if (deliveryType == "Outgoing" && (!_delivery?.IsProcessed ?? true))
            {
                var validationErrors = new System.Text.StringBuilder();
                
                // Groepeer items per product en check totale hoeveelheid
                var productGroups = _deliveryItems.GroupBy(i => i.ProductId);
                foreach (var group in productGroups)
                {
                    var product = await _context.Products.FindAsync(group.Key);
                    if (product != null)
                    {
                        int totalQuantity = group.Sum(i => i.Quantity);
                        
                        if (product.StockQuantity < totalQuantity)
                        {
                            validationErrors.AppendLine(
                                $"- {product.ProductName}: voorraad {product.StockQuantity}, nodig {totalQuantity}");
                        }
                    }
                }

                if (validationErrors.Length > 0)
                {
                    await DisplayAlert("Onvoldoende voorraad",
                        "De volgende producten hebben onvoldoende voorraad:\n\n" + 
                        validationErrors.ToString() +
                        "\nDe levering kan niet worden opgeslagen.",
                        "OK");
                    return;
                }
            }
            
            // Update delivery properties
            if (_delivery == null) return;
            
            _delivery.DeliveryType = deliveryType;
            _delivery.ReferenceNumber = ReferenceNumberEntry.Text.Trim();
            
            if (deliveryType == "Incoming")
            {
                _delivery.SupplierId = (SupplierPicker.SelectedItem as Supplier)?.SupplierId;
                _delivery.CustomerId = null;
            }
            else
            {
                _delivery.CustomerId = (CustomerPicker.SelectedItem as Customer)?.CustomerId;
                _delivery.SupplierId = null;
            }
            
            _delivery.VehicleId = (VehiclePicker.SelectedItem as Vehicle)?.VehicleId;
            _delivery.ExpectedDeliveryDate = ExpectedDatePicker.Date;
            _delivery.Status = StatusPicker.SelectedItem?.ToString() ?? "Gepland";
            _delivery.Notes = NotesEditor.Text?.Trim() ?? string.Empty;
            _delivery.TotalAmount = _deliveryItems.Sum(i => i.SubTotal);
            
            bool isNewDelivery = !deliveryId.HasValue || deliveryId.Value == 0;
            
            // Save delivery
            if (isNewDelivery)
            {
                // Create new
                _delivery.CreatedDate = DateTime.Now;
                await _context.Deliveries.AddAsync(_delivery);
                await _context.SaveChangesAsync();
                
                Debug.WriteLine($"Created new delivery with ID: {_delivery.DeliveryId}");
                
                // Add items
                foreach (var itemVM in _deliveryItems)
                {
                    var deliveryItem = new DeliveryItem
                    {
                        DeliveryId = _delivery.DeliveryId,
                        ProductId = itemVM.ProductId,
                        Quantity = itemVM.Quantity,
                        UnitPrice = itemVM.UnitPrice,
                        IsProcessed = false
                    };
                    
                    await _context.DeliveryItems.AddAsync(deliveryItem);
                }
                
                await _context.SaveChangesAsync();
                Debug.WriteLine($"Created {_deliveryItems.Count} delivery items");
            }
            else
            {
                // Update existing
                _context.Deliveries.Update(_delivery);
                
                // Remove old items
                var existingItems = await _context.DeliveryItems
                    .Where(di => di.DeliveryId == _delivery.DeliveryId)
                    .ToListAsync();
                
                _context.DeliveryItems.RemoveRange(existingItems);
                
                Debug.WriteLine($"Updated delivery {_delivery.DeliveryId}, removed {existingItems.Count} old items");
                
                // Add new items
                foreach (var itemVM in _deliveryItems)
                {
                    var deliveryItem = new DeliveryItem
                    {
                        DeliveryId = _delivery.DeliveryId,
                        ProductId = itemVM.ProductId,
                        Quantity = itemVM.Quantity,
                        UnitPrice = itemVM.UnitPrice,
                        IsProcessed = false
                    };
                    
                    await _context.DeliveryItems.AddAsync(deliveryItem);
                }
                
                await _context.SaveChangesAsync();
                Debug.WriteLine($"Saved {_deliveryItems.Count} delivery items");
            }
            
            await DisplayAlert("Succes", 
                isNewDelivery ? "Nieuwe levering aangemaakt!" : "Levering bijgewerkt!", 
                "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnSaveClicked ERROR: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"Fout bij opslaan: {ex.Message}", "OK");
        }
    }
    
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

// ViewModel class for DeliveryItems in the UI
public class DeliveryItemViewModel
{
    public int DeliveryItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}
