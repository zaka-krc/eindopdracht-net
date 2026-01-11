using Microsoft.EntityFrameworkCore;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_App.Data;
using SuntoryManagementSystem_App.Services.Models;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.Services;

/// <summary>
/// Service voor online/offline synchronisatie.
/// Synchroniseert data tussen de lokale SQLite database en de remote API.
/// </summary>
public class SyncService
{
    private readonly LocalDbContext _localContext;
    private readonly ApiService _apiService;
    private readonly ConnectivityService _connectivityService;
    private readonly AuthService _authService;
    
    private bool _isSyncing;
    private DateTime _lastSyncTime = DateTime.MinValue;
    private Timer? _autoSyncTimer;
    private const int AUTO_SYNC_INTERVAL_MINUTES = 5;
    
    public SyncService(
        LocalDbContext localContext, 
        ApiService apiService, 
        ConnectivityService connectivityService,
        AuthService authService)
    {
        _localContext = localContext;
        _apiService = apiService;
        _connectivityService = connectivityService;
        _authService = authService;
        
        // Luister naar connectiviteitswijzigingen
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
        
        // Start automatic sync timer
        StartAutoSyncTimer();
    }
    
    /// <summary>
    /// Start de automatische sync timer (elke 5 minuten)
    /// </summary>
    private void StartAutoSyncTimer()
    {
        _autoSyncTimer = new Timer(
            async _ => await AutoSyncCallback(),
            null,
            TimeSpan.FromMinutes(AUTO_SYNC_INTERVAL_MINUTES),
            TimeSpan.FromMinutes(AUTO_SYNC_INTERVAL_MINUTES));
        
        Debug.WriteLine($"SyncService: Auto-sync timer started (interval: {AUTO_SYNC_INTERVAL_MINUTES} minutes)");
    }
    
    /// <summary>
    /// Callback voor automatische synchronisatie
    /// </summary>
    private async Task AutoSyncCallback()
    {
        try
        {
            // Check of we authenticated zijn en online
            bool isAuthenticated = await _authService.IsAuthenticatedAsync();
            
            if (isAuthenticated && _connectivityService.IsConnected && !_isSyncing)
            {
                Debug.WriteLine("? Auto-sync triggered");
                var result = await SyncAllAsync();
                
                if (result.Success)
                {
                    Debug.WriteLine("? Auto-sync completed successfully");
                }
                else
                {
                    Debug.WriteLine($"?? Auto-sync completed with warnings: {result.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"? Auto-sync error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Event dat wordt getriggerd wanneer synchronisatie status verandert
    /// </summary>
    public event EventHandler<SyncStatus>? SyncStatusChanged;
    
    /// <summary>
    /// Geeft aan of er momenteel gesynchroniseerd wordt
    /// </summary>
    public bool IsSyncing => _isSyncing;
    
    /// <summary>
    /// Tijdstip van laatste succesvolle synchronisatie
    /// </summary>
    public DateTime LastSyncTime => _lastSyncTime;
    
    private async void OnConnectivityChanged(object? sender, bool isConnected)
    {
        if (isConnected)
        {
            Debug.WriteLine("SyncService: Connection restored, starting sync...");
            
            // Check authentication before syncing
            bool isAuthenticated = await _authService.IsAuthenticatedAsync();
            if (isAuthenticated)
            {
                await SyncAllAsync();
            }
            else
            {
                Debug.WriteLine("SyncService: Not authenticated, skipping sync");
            }
        }
    }
    
    /// <summary>
    /// Synchroniseert alle data met de server (BIDIRECTIONEEL: upload eerst, dan download)
    /// </summary>
    public async Task<SyncResult> SyncAllAsync()
    {
        if (_isSyncing)
        {
            return new SyncResult { Success = false, Message = "Synchronisatie is al bezig" };
        }
        
        if (!_connectivityService.IsConnected)
        {
            return new SyncResult { Success = false, Message = "Geen internetverbinding" };
        }
        
        // Check authentication
        bool isAuthenticated = await _authService.IsAuthenticatedAsync();
        if (!isAuthenticated)
        {
            return new SyncResult { Success = false, Message = "Niet ingelogd - kan niet synchroniseren" };
        }
        
        _isSyncing = true;
        SyncStatusChanged?.Invoke(this, SyncStatus.Syncing);
        
        var result = new SyncResult { Success = true };
        var errors = new List<string>();
        
        try
        {
            Debug.WriteLine("SyncService: Starting full bidirectional sync...");
            
            // 1. Synchroniseer Suppliers (alleen downloaden - read-only in MAUI)
            await SyncSuppliersAsync(errors);
            
            // 2. Synchroniseer Products (BEIDE RICHTINGEN: upload eerst, dan download)
            await SyncProductsAsync(errors);
            
            // 3. Synchroniseer Customers (BEIDE RICHTINGEN)
            await SyncCustomersAsync(errors);
            
            // 4. Synchroniseer Deliveries (BEIDE RICHTINGEN)
            await SyncDeliveriesAsync(errors);
            
            // 5. Synchroniseer Vehicles (alleen downloaden - read-only in MAUI)
            await SyncVehiclesAsync(errors);
            
            if (errors.Count > 0)
            {
                result.Success = false;
                result.Message = string.Join("; ", errors);
            }
            else
            {
                result.Message = "Synchronisatie succesvol";
                _lastSyncTime = DateTime.Now;
            }
            
            Debug.WriteLine($"SyncService: Sync completed. Success: {result.Success}");
            Debug.WriteLine($"?? Sync Summary - Errors: {errors.Count}, Last Sync: {_lastSyncTime:HH:mm:ss}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SyncService: Sync failed with exception: {ex.Message}");
            result.Success = false;
            result.Message = $"Synchronisatie fout: {ex.Message}";
        }
        finally
        {
            _isSyncing = false;
            SyncStatusChanged?.Invoke(this, result.Success ? SyncStatus.Completed : SyncStatus.Failed);
        }
        
        return result;
    }
    
    #region Products Sync (BIDIRECTIONAL)
    
    private async Task SyncProductsAsync(List<string> errors)
    {
        try
        {
            Debug.WriteLine("SyncService: Syncing products (bidirectional)...");
            
            // STAP 1: Upload nieuwe locale producten naar server
            var localProducts = await _localContext.Products.AsNoTracking().ToListAsync();
            
            // Vind producten die lokaal zijn aangemaakt (negatieve ID's of ID's die niet op server bestaan)
            var localOnlyProducts = localProducts
                .Where(p => p.ProductId <= 0 || !p.IsDeleted) // Negatieve IDs = lokaal aangemaakt
                .ToList();
            
            int uploadCount = 0;
            foreach (var localProduct in localOnlyProducts)
            {
                if (localProduct.ProductId <= 0)
                {
                    // Nieuw lokaal product - upload naar server
                    Debug.WriteLine($"?? Uploading NEW product '{localProduct.ProductName}' (Local ID: {localProduct.ProductId}) to server...");
                    
                    try
                    {
                        var serverProduct = await _apiService.CreateProductAsync(localProduct);
                        
                        // Remove old entity with negative ID and add new one with server ID
                        int oldId = localProduct.ProductId;
                        var trackedProduct = await _localContext.Products.FindAsync(oldId);
                        if (trackedProduct != null)
                        {
                            _localContext.Products.Remove(trackedProduct);
                            await _localContext.SaveChangesAsync();
                            
                            // Add the server product with the correct ID
                            _localContext.Products.Add(serverProduct);
                            await _localContext.SaveChangesAsync();
                            
                            uploadCount++;
                            Debug.WriteLine($"? Product uploaded successfully! Local ID {oldId} ? Server ID {serverProduct.ProductId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"? Failed to upload product '{localProduct.ProductName}': {ex.Message}");
                        errors.Add($"Upload product '{localProduct.ProductName}' mislukt: {ex.Message}");
                    }
                }
            }
            
            // STAP 2: Download alle producten van server (inclusief net geüploade)
            var remoteProducts = await _apiService.GetProductsAsync();
            
            // Refresh local products list na upload (untracked)
            localProducts = await _localContext.Products.AsNoTracking().ToListAsync();
            
            int addCount = 0;
            int updateCount = 0;
            foreach (var remoteProduct in remoteProducts)
            {
                var localProduct = localProducts.FirstOrDefault(p => p.ProductId == remoteProduct.ProductId);
                
                if (localProduct == null)
                {
                    // Nieuw product van server - toevoegen
                    _localContext.Products.Add(remoteProduct);
                    addCount++;
                    Debug.WriteLine($"? Downloaded new product '{remoteProduct.ProductName}' from server");
                }
                else if (!localProduct.IsDeleted)
                {
                    // Bestaand product - update met server data (server is master)
                    var trackedProduct = await _localContext.Products.FindAsync(remoteProduct.ProductId);
                    if (trackedProduct != null)
                    {
                        UpdateLocalProduct(trackedProduct, remoteProduct);
                        updateCount++;
                        Debug.WriteLine($"?? Updated product '{remoteProduct.ProductName}' from server");
                    }
                }
            }
            
            // Verwijder lokale producten die niet meer op server bestaan
            var remoteIds = remoteProducts.Select(p => p.ProductId).ToHashSet();
            var toDelete = localProducts
                .Where(p => p.ProductId > 0 && !remoteIds.Contains(p.ProductId) && !p.IsDeleted)
                .ToList();
            
            foreach (var product in toDelete)
            {
                // Soft delete
                var trackedProduct = await _localContext.Products.FindAsync(product.ProductId);
                if (trackedProduct != null)
                {
                    trackedProduct.IsDeleted = true;
                    trackedProduct.DeletedDate = DateTime.Now;
                    Debug.WriteLine($"??? Soft deleted product '{trackedProduct.ProductName}' (removed from server)");
                }
            }
            
            await _localContext.SaveChangesAsync();
            Debug.WriteLine($"?? Products sync completed: ?{uploadCount} uploaded, ?{addCount} added, ??{updateCount} updated, ???{toDelete.Count} deleted. Total: {remoteProducts.Count} remote products");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"? Products sync error: {ex.Message}");
            errors.Add($"Products sync fout: {ex.Message}");
        }
    }
    
    private static void UpdateLocalProduct(Product local, Product remote)
    {
        local.ProductName = remote.ProductName;
        local.Description = remote.Description;
        local.SKU = remote.SKU;
        local.Category = remote.Category;
        local.PurchasePrice = remote.PurchasePrice;
        local.SellingPrice = remote.SellingPrice;
        local.StockQuantity = remote.StockQuantity;
        local.MinimumStock = remote.MinimumStock;
        local.SupplierId = remote.SupplierId;
        local.IsActive = remote.IsActive;
        local.IsDeleted = remote.IsDeleted;
        local.DeletedDate = remote.DeletedDate;
    }
    
    #endregion
    
    #region Customers Sync (BIDIRECTIONAL)
    
    private async Task SyncCustomersAsync(List<string> errors)
    {
        try
        {
            Debug.WriteLine("SyncService: Syncing customers (bidirectional)...");
            
            // STAP 1: Upload nieuwe lokale customers naar server
            var localCustomers = await _localContext.Customers.AsNoTracking().ToListAsync();
            
            var localOnlyCustomers = localCustomers
                .Where(c => c.CustomerId <= 0 || !c.IsDeleted)
                .ToList();
            
            int uploadCount = 0;
            foreach (var localCustomer in localOnlyCustomers)
            {
                if (localCustomer.CustomerId <= 0)
                {
                    Debug.WriteLine($"?? Uploading NEW customer '{localCustomer.CustomerName}' (Local ID: {localCustomer.CustomerId}) to server...");
                    
                    try
                    {
                        var serverCustomer = await _apiService.CreateCustomerAsync(localCustomer);
                        
                        // Remove old entity and add new one with server ID
                        int oldId = localCustomer.CustomerId;
                        var trackedCustomer = await _localContext.Customers.FindAsync(oldId);
                        if (trackedCustomer != null)
                        {
                            _localContext.Customers.Remove(trackedCustomer);
                            await _localContext.SaveChangesAsync();
                            
                            _localContext.Customers.Add(serverCustomer);
                            await _localContext.SaveChangesAsync();
                            
                            uploadCount++;
                            Debug.WriteLine($"? Customer uploaded successfully! Local ID {oldId} ? Server ID {serverCustomer.CustomerId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"? Failed to upload customer '{localCustomer.CustomerName}': {ex.Message}");
                        errors.Add($"Upload customer '{localCustomer.CustomerName}' mislukt: {ex.Message}");
                    }
                }
            }
            
            // STAP 2: Download alle customers van server
            var remoteCustomers = await _apiService.GetCustomersAsync();
            localCustomers = await _localContext.Customers.AsNoTracking().ToListAsync();
            
            int addCount = 0;
            int updateCount = 0;
            foreach (var remoteCustomer in remoteCustomers)
            {
                var localCustomer = localCustomers.FirstOrDefault(c => c.CustomerId == remoteCustomer.CustomerId);
                
                if (localCustomer == null)
                {
                    _localContext.Customers.Add(remoteCustomer);
                    addCount++;
                    Debug.WriteLine($"? Downloaded new customer '{remoteCustomer.CustomerName}' from server");
                }
                else if (!localCustomer.IsDeleted)
                {
                    var trackedCustomer = await _localContext.Customers.FindAsync(remoteCustomer.CustomerId);
                    if (trackedCustomer != null)
                    {
                        UpdateLocalCustomer(trackedCustomer, remoteCustomer);
                        updateCount++;
                        Debug.WriteLine($"?? Updated customer '{remoteCustomer.CustomerName}' from server");
                    }
                }
            }
            
            var remoteIds = remoteCustomers.Select(c => c.CustomerId).ToHashSet();
            var toDelete = localCustomers
                .Where(c => c.CustomerId > 0 && !remoteIds.Contains(c.CustomerId) && !c.IsDeleted)
                .ToList();
            
            foreach (var customer in toDelete)
            {
                var trackedCustomer = await _localContext.Customers.FindAsync(customer.CustomerId);
                if (trackedCustomer != null)
                {
                    trackedCustomer.IsDeleted = true;
                    trackedCustomer.DeletedDate = DateTime.Now;
                }
            }
            
            await _localContext.SaveChangesAsync();
            Debug.WriteLine($"?? Customers sync completed: ?{uploadCount} uploaded, ?{addCount} added, ??{updateCount} updated, ???{toDelete.Count} deleted. Total: {remoteCustomers.Count}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"? Customers sync error: {ex.Message}");
            errors.Add($"Customers sync fout: {ex.Message}");
        }
    }
    
    private static void UpdateLocalCustomer(Customer local, Customer remote)
    {
        local.CustomerName = remote.CustomerName;
        local.ContactPerson = remote.ContactPerson;
        local.Email = remote.Email;
        local.PhoneNumber = remote.PhoneNumber;
        local.Address = remote.Address;
        local.City = remote.City;
        local.PostalCode = remote.PostalCode;
        local.CustomerType = remote.CustomerType;
        local.Status = remote.Status;
        local.Notes = remote.Notes;
        local.IsDeleted = remote.IsDeleted;
        local.DeletedDate = remote.DeletedDate;
    }
    
    #endregion
    
    #region Deliveries Sync (BIDIRECTIONAL)
    
    private async Task SyncDeliveriesAsync(List<string> errors)
    {
        try
        {
            Debug.WriteLine("SyncService: Syncing deliveries (bidirectional)...");
            
            // STAP 1: Upload nieuwe lokale deliveries naar server
            var localDeliveries = await _localContext.Deliveries.AsNoTracking().ToListAsync();
            
            var localOnlyDeliveries = localDeliveries
                .Where(d => d.DeliveryId <= 0 || !d.IsDeleted)
                .ToList();
            
            int uploadCount = 0;
            foreach (var localDelivery in localOnlyDeliveries)
            {
                if (localDelivery.DeliveryId <= 0)
                {
                    Debug.WriteLine($"?? Uploading NEW delivery '{localDelivery.ReferenceNumber}' (Local ID: {localDelivery.DeliveryId}) to server...");
                    
                    try
                    {
                        var serverDelivery = await _apiService.CreateDeliveryAsync(localDelivery);
                        
                        // Remove old entity and add new one with server ID
                        int oldId = localDelivery.DeliveryId;
                        var trackedDelivery = await _localContext.Deliveries.FindAsync(oldId);
                        if (trackedDelivery != null)
                        {
                            _localContext.Deliveries.Remove(trackedDelivery);
                            await _localContext.SaveChangesAsync();
                            
                            _localContext.Deliveries.Add(serverDelivery);
                            await _localContext.SaveChangesAsync();
                            
                            uploadCount++;
                            Debug.WriteLine($"? Delivery uploaded successfully! Local ID {oldId} ? Server ID {serverDelivery.DeliveryId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"? Failed to upload delivery '{localDelivery.ReferenceNumber}': {ex.Message}");
                        errors.Add($"Upload delivery '{localDelivery.ReferenceNumber}' mislukt: {ex.Message}");
                    }
                }
            }
            
            // STAP 2: Download alle deliveries van server
            var remoteDeliveries = await _apiService.GetDeliveriesAsync();
            localDeliveries = await _localContext.Deliveries.AsNoTracking().ToListAsync();
            
            int addCount = 0;
            int updateCount = 0;
            foreach (var remoteDelivery in remoteDeliveries)
            {
                var localDelivery = localDeliveries.FirstOrDefault(d => d.DeliveryId == remoteDelivery.DeliveryId);
                
                if (localDelivery == null)
                {
                    _localContext.Deliveries.Add(remoteDelivery);
                    addCount++;
                    Debug.WriteLine($"? Downloaded new delivery '{remoteDelivery.ReferenceNumber}' from server");
                }
                else if (!localDelivery.IsDeleted)
                {
                    var trackedDelivery = await _localContext.Deliveries.FindAsync(remoteDelivery.DeliveryId);
                    if (trackedDelivery != null)
                    {
                        UpdateLocalDelivery(trackedDelivery, remoteDelivery);
                        updateCount++;
                        Debug.WriteLine($"?? Updated delivery '{remoteDelivery.ReferenceNumber}' from server");
                    }
                }
            }
            
            var remoteIds = remoteDeliveries.Select(d => d.DeliveryId).ToHashSet();
            var toDelete = localDeliveries
                .Where(d => d.DeliveryId > 0 && !remoteIds.Contains(d.DeliveryId) && !d.IsDeleted)
                .ToList();
            
            foreach (var delivery in toDelete)
            {
                var trackedDelivery = await _localContext.Deliveries.FindAsync(delivery.DeliveryId);
                if (trackedDelivery != null)
                {
                    trackedDelivery.IsDeleted = true;
                    trackedDelivery.DeletedDate = DateTime.Now;
                }
            }
            
            await _localContext.SaveChangesAsync();
            Debug.WriteLine($"?? Deliveries sync completed: ?{uploadCount} uploaded, ?{addCount} added, ??{updateCount} updated, ???{toDelete.Count} deleted. Total: {remoteDeliveries.Count}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"? Deliveries sync error: {ex.Message}");
            errors.Add($"Deliveries sync fout: {ex.Message}");
        }
    }
    
    private static void UpdateLocalDelivery(Delivery local, Delivery remote)
    {
        local.DeliveryType = remote.DeliveryType;
        local.Status = remote.Status;
        local.ReferenceNumber = remote.ReferenceNumber;
        local.ExpectedDeliveryDate = remote.ExpectedDeliveryDate;
        local.ActualDeliveryDate = remote.ActualDeliveryDate;
        local.SupplierId = remote.SupplierId;
        local.CustomerId = remote.CustomerId;
        local.VehicleId = remote.VehicleId;
        local.TotalAmount = remote.TotalAmount;
        local.IsProcessed = remote.IsProcessed;
        local.Notes = remote.Notes;
        local.IsDeleted = remote.IsDeleted;
        local.DeletedDate = remote.DeletedDate;
    }
    
    #endregion
    
    #region Suppliers Sync (READ-ONLY from server)
    
    private async Task SyncSuppliersAsync(List<string> errors)
    {
        try
        {
            Debug.WriteLine("SyncService: Syncing suppliers (read-only from server)...");
            
            var remoteSuppliers = await _apiService.GetSuppliersAsync();
            var localSuppliers = await _localContext.Suppliers.AsNoTracking().ToListAsync();
            
            int addCount = 0;
            int updateCount = 0;
            foreach (var remoteSupplier in remoteSuppliers)
            {
                var localSupplier = localSuppliers.FirstOrDefault(s => s.SupplierId == remoteSupplier.SupplierId);
                
                if (localSupplier == null)
                {
                    _localContext.Suppliers.Add(remoteSupplier);
                    addCount++;
                }
                else
                {
                    var trackedSupplier = await _localContext.Suppliers.FindAsync(remoteSupplier.SupplierId);
                    if (trackedSupplier != null)
                    {
                        UpdateLocalSupplier(trackedSupplier, remoteSupplier);
                        updateCount++;
                    }
                }
            }
            
            var remoteIds = remoteSuppliers.Select(s => s.SupplierId).ToHashSet();
            var toDelete = localSuppliers.Where(s => !remoteIds.Contains(s.SupplierId)).ToList();
            
            foreach (var supplier in toDelete)
            {
                var trackedSupplier = await _localContext.Suppliers.FindAsync(supplier.SupplierId);
                if (trackedSupplier != null)
                {
                    _localContext.Suppliers.Remove(trackedSupplier);
                }
            }
            
            await _localContext.SaveChangesAsync();
            Debug.WriteLine($"?? Suppliers sync completed: ?{addCount} added, ??{updateCount} updated, ???{toDelete.Count} deleted. Total: {remoteSuppliers.Count}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"? Suppliers sync error: {ex.Message}");
            errors.Add($"Suppliers sync fout: {ex.Message}");
        }
    }
    
    private static void UpdateLocalSupplier(Supplier local, Supplier remote)
    {
        local.SupplierName = remote.SupplierName;
        local.ContactPerson = remote.ContactPerson;
        local.Email = remote.Email;
        local.PhoneNumber = remote.PhoneNumber;
        local.Address = remote.Address;
        local.City = remote.City;
        local.PostalCode = remote.PostalCode;
        local.Status = remote.Status;
        local.Notes = remote.Notes;
        local.IsDeleted = remote.IsDeleted;
        local.DeletedDate = remote.DeletedDate;
    }
    
    #endregion
    
    #region Vehicles Sync (READ-ONLY from server)
    
    private async Task SyncVehiclesAsync(List<string> errors)
    {
        try
        {
            Debug.WriteLine("SyncService: Syncing vehicles (read-only from server)...");
            
            var remoteVehicles = await _apiService.GetVehiclesAsync();
            var localVehicles = await _localContext.Vehicles.AsNoTracking().ToListAsync();
            
            int addCount = 0;
            int updateCount = 0;
            foreach (var remoteVehicle in remoteVehicles)
            {
                var localVehicle = localVehicles.FirstOrDefault(v => v.VehicleId == remoteVehicle.VehicleId);
                
                if (localVehicle == null)
                {
                    _localContext.Vehicles.Add(remoteVehicle);
                    addCount++;
                }
                else
                {
                    var trackedVehicle = await _localContext.Vehicles.FindAsync(remoteVehicle.VehicleId);
                    if (trackedVehicle != null)
                    {
                        UpdateLocalVehicle(trackedVehicle, remoteVehicle);
                        updateCount++;
                    }
                }
            }
            
            var remoteIds = remoteVehicles.Select(v => v.VehicleId).ToHashSet();
            var toDelete = localVehicles.Where(v => !remoteIds.Contains(v.VehicleId)).ToList();
            
            foreach (var vehicle in toDelete)
            {
                var trackedVehicle = await _localContext.Vehicles.FindAsync(vehicle.VehicleId);
                if (trackedVehicle != null)
                {
                    _localContext.Vehicles.Remove(trackedVehicle);
                }
            }
            
            await _localContext.SaveChangesAsync();
            Debug.WriteLine($"?? Vehicles sync completed: ?{addCount} added, ??{updateCount} updated, ???{toDelete.Count} deleted. Total: {remoteVehicles.Count}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"? Vehicles sync error: {ex.Message}");
            errors.Add($"Vehicles sync fout: {ex.Message}");
        }
    }
    
    private static void UpdateLocalVehicle(Vehicle local, Vehicle remote)
    {
        local.LicensePlate = remote.LicensePlate;
        local.Brand = remote.Brand;
        local.Model = remote.Model;
        local.Capacity = remote.Capacity;
        local.IsAvailable = remote.IsAvailable;
        local.IsDeleted = remote.IsDeleted;
        local.DeletedDate = remote.DeletedDate;
    }
    
    #endregion
    
    #region Partial Sync Methods
    
    /// <summary>
    /// Synchroniseert alleen producten (bidirectioneel)
    /// </summary>
    public async Task<SyncResult> SyncProductsOnlyAsync()
    {
        if (!_connectivityService.IsConnected)
        {
            return new SyncResult { Success = false, Message = "Geen internetverbinding" };
        }
        
        var errors = new List<string>();
        await SyncProductsAsync(errors);
        
        return new SyncResult
        {
            Success = errors.Count == 0,
            Message = errors.Count == 0 ? "Producten gesynchroniseerd" : string.Join("; ", errors)
        };
    }
    
    /// <summary>
    /// Synchroniseert alleen customers (bidirectioneel)
    /// </summary>
    public async Task<SyncResult> SyncCustomersOnlyAsync()
    {
        if (!_connectivityService.IsConnected)
        {
            return new SyncResult { Success = false, Message = "Geen internetverbinding" };
        }
        
        var errors = new List<string>();
        await SyncCustomersAsync(errors);
        
        return new SyncResult
        {
            Success = errors.Count == 0,
            Message = errors.Count == 0 ? "Klanten gesynchroniseerd" : string.Join("; ", errors)
        };
    }
    
    /// <summary>
    /// Synchroniseert alleen deliveries (bidirectioneel)
    /// </summary>
    public async Task<SyncResult> SyncDeliveriesOnlyAsync()
    {
        if (!_connectivityService.IsConnected)
        {
            return new SyncResult { Success = false, Message = "Geen internetverbinding" };
        }
        
        var errors = new List<string>();
        await SyncDeliveriesAsync(errors);
        
        return new SyncResult
        {
            Success = errors.Count == 0,
            Message = errors.Count == 0 ? "Leveringen gesynchroniseerd" : string.Join("; ", errors)
        };
    }
    
    #endregion
}
