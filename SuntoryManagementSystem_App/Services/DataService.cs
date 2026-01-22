using Microsoft.EntityFrameworkCore;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_App.Data;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.Services;

/// <summary>
/// Centrale service voor data operaties.
/// Synchroniseert DIRECT met de server wanneer online.
/// Slaat lokaal op wanneer offline (wordt later gesynchroniseerd).
/// </summary>
public class DataService
{
    private readonly LocalDbContext _localContext;
    private readonly ApiService _apiService;
    private readonly ConnectivityService _connectivityService;
    private readonly AuthService _authService;

    // Event om aan te geven dat data is gewijzigd
    public event EventHandler? DataChanged;

    public DataService(
        LocalDbContext localContext,
        ApiService apiService,
        ConnectivityService connectivityService,
        AuthService authService)
    {
        _localContext = localContext;
        _apiService = apiService;
        _connectivityService = connectivityService;
        _authService = authService;
    }

    /// <summary>
    /// Trigger het DataChanged event om views te notificeren
    /// </summary>
    private void OnDataChanged()
    {
        Debug.WriteLine("DataService: Triggering DataChanged event");
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Check of we online zijn EN authenticated
    /// </summary>
    private async Task<bool> CanSyncWithServerAsync()
    {
        if (!_connectivityService.IsConnected)
        {
            Debug.WriteLine("DataService: Offline - changes will be stored locally");
            return false;
        }

        bool isAuthenticated = await _authService.IsAuthenticatedAsync();
        if (!isAuthenticated)
        {
            Debug.WriteLine("DataService: Not authenticated - changes will be stored locally");
            return false;
        }

        return true;
    }

    #region Products

    /// <summary>
    /// Maakt een nieuw product aan - lokaal EN op server indien online
    /// </summary>
    public async Task<Product> CreateProductAsync(Product product)
    {
        Debug.WriteLine($"DataService: Creating product '{product.ProductName}'");

        if (await CanSyncWithServerAsync())
        {
            try
            {
                // Stuur direct naar server
                var serverProduct = await _apiService.CreateProductAsync(product);
                Debug.WriteLine($"DataService: Product created on server with ID {serverProduct.ProductId}");

                // Sla op in lokale database met server ID
        _localContext.Products.Add(serverProduct);
                await _localContext.SaveChangesAsync();
                Debug.WriteLine($"DataService: Product saved locally with server ID {serverProduct.ProductId}");

                OnDataChanged();
                return serverProduct;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DataService: Server error, saving locally: {ex.Message}");
                // Fall through to local save
            }
        }

        // Offline of server error: sla lokaal op met negatief ID
        product.ProductId = -(int)(DateTime.Now.Ticks % int.MaxValue);
        product.CreatedDate = DateTime.Now;
        
        _localContext.Products.Add(product);
        await _localContext.SaveChangesAsync();
        Debug.WriteLine($"DataService: Product saved locally with temporary ID {product.ProductId}");

        OnDataChanged();
        return product;
    }

    /// <summary>
    /// Update een bestaand product - lokaal EN op server indien online
    /// </summary>
    public async Task UpdateProductAsync(Product product)
    {
        Debug.WriteLine($"DataService: Updating product '{product.ProductName}' (ID: {product.ProductId})");

        // Update lokaal eerst
        _localContext.Products.Update(product);
        await _localContext.SaveChangesAsync();
        Debug.WriteLine($"DataService: Product updated locally");
        OnDataChanged();

        if (await CanSyncWithServerAsync())
        {
            // Alleen server updaten als het een server product is (positief ID)
            if (product.ProductId > 0)
            {
                try
                {
                    await _apiService.UpdateProductAsync(product);
                    Debug.WriteLine($"DataService: Product updated on server");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DataService: Server update failed (will sync later): {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"DataService: Product has local ID ({product.ProductId}), will be uploaded on next sync");
            }
        }
    }

    /// <summary>
    /// Verwijder een product (soft delete) - lokaal EN op server indien online
    /// </summary>
    public async Task DeleteProductAsync(Product product)
    {
        Debug.WriteLine($"DataService: Deleting product '{product.ProductName}' (ID: {product.ProductId})");

        // Soft delete lokaal
        product.IsDeleted = true;
        product.DeletedDate = DateTime.Now;
        _localContext.Products.Update(product);
        await _localContext.SaveChangesAsync();
        Debug.WriteLine($"DataService: Product soft-deleted locally");
        OnDataChanged();

        if (await CanSyncWithServerAsync())
        {
            // Alleen server updaten als het een server product is (positief ID)
            if (product.ProductId > 0)
            {
                try
                {
                    await _apiService.DeleteProductAsync(product.ProductId);
                    Debug.WriteLine($"DataService: Product deleted on server");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DataService: Server delete failed (will sync later): {ex.Message}");
                }
            }
        }
    }

    #endregion

    #region Customers

    /// <summary>
    /// Maakt een nieuwe klant aan - lokaal EN op server indien online
    /// </summary>
    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        Debug.WriteLine($"DataService: Creating customer '{customer.CustomerName}'");

        if (await CanSyncWithServerAsync())
        {
            try
            {
                var serverCustomer = await _apiService.CreateCustomerAsync(customer);
                Debug.WriteLine($"DataService: Customer created on server with ID {serverCustomer.CustomerId}");

        _localContext.Customers.Add(serverCustomer);
                await _localContext.SaveChangesAsync();
                Debug.WriteLine($"DataService: Customer saved locally with server ID {serverCustomer.CustomerId}");

                OnDataChanged();
                return serverCustomer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DataService: Server error, saving locally: {ex.Message}");
            }
        }

        // Offline of server error
        customer.CustomerId = -(int)(DateTime.Now.Ticks % int.MaxValue);
        customer.CreatedDate = DateTime.Now;
        
        _localContext.Customers.Add(customer);
        await _localContext.SaveChangesAsync();
        Debug.WriteLine($"DataService: Customer saved locally with temporary ID {customer.CustomerId}");

        OnDataChanged();
        return customer;
    }

    /// <summary>
    /// Update een bestaande klant - lokaal EN op server indien online
    /// </summary>
    public async Task UpdateCustomerAsync(Customer customer)
    {
        Debug.WriteLine($"DataService: Updating customer '{customer.CustomerName}' (ID: {customer.CustomerId})");

        _localContext.Customers.Update(customer);
        await _localContext.SaveChangesAsync();
        Debug.WriteLine($"DataService: Customer updated locally");
        OnDataChanged();

        if (await CanSyncWithServerAsync())
        {
            if (customer.CustomerId > 0)
            {
                try
                {
                    await _apiService.UpdateCustomerAsync(customer);
                    Debug.WriteLine($"DataService: Customer updated on server");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DataService: Server update failed (will sync later): {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Verwijder een klant (soft delete) - lokaal EN op server indien online
    /// </summary>
    public async Task DeleteCustomerAsync(Customer customer)
    {
        Debug.WriteLine($"DataService: Deleting customer '{customer.CustomerName}' (ID: {customer.CustomerId})");

        customer.IsDeleted = true;
        customer.DeletedDate = DateTime.Now;
        _localContext.Customers.Update(customer);
        await _localContext.SaveChangesAsync();
        Debug.WriteLine($"DataService: Customer soft-deleted locally");
        OnDataChanged();

        if (await CanSyncWithServerAsync())
        {
            if (customer.CustomerId > 0)
            {
                try
                {
                    await _apiService.DeleteCustomerAsync(customer.CustomerId);
                    Debug.WriteLine($"DataService: Customer deleted on server");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DataService: Server delete failed (will sync later): {ex.Message}");
                }
            }
        }
    }

    #endregion

    #region Deliveries

    /// <summary>
    /// Maakt een nieuwe levering aan - lokaal EN op server indien online
    /// </summary>
    public async Task<Delivery> CreateDeliveryAsync(Delivery delivery)
    {
        Debug.WriteLine($"DataService: Creating delivery '{delivery.ReferenceNumber}'");

        if (await CanSyncWithServerAsync())
        {
            try
            {
                var serverDelivery = await _apiService.CreateDeliveryAsync(delivery);
                Debug.WriteLine($"DataService: Delivery created on server with ID {serverDelivery.DeliveryId}");

        _localContext.Deliveries.Add(serverDelivery);
                await _localContext.SaveChangesAsync();
                Debug.WriteLine($"DataService: Delivery saved locally with server ID {serverDelivery.DeliveryId}");

                OnDataChanged();
                return serverDelivery;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DataService: Server error, saving locally: {ex.Message}");
            }
        }

        // Offline of server error
        delivery.DeliveryId = -(int)(DateTime.Now.Ticks % int.MaxValue);
        delivery.CreatedDate = DateTime.Now;
        
        _localContext.Deliveries.Add(delivery);
        await _localContext.SaveChangesAsync();
        Debug.WriteLine($"DataService: Delivery saved locally with temporary ID {delivery.DeliveryId}");

        OnDataChanged();
        return delivery;
    }

    /// <summary>
    /// Update een bestaande levering - lokaal EN op server indien online
    /// </summary>
    public async Task UpdateDeliveryAsync(Delivery delivery)
    {
        Debug.WriteLine($"DataService: Updating delivery '{delivery.ReferenceNumber}' (ID: {delivery.DeliveryId})");

        _localContext.Deliveries.Update(delivery);
        await _localContext.SaveChangesAsync();
        Debug.WriteLine($"DataService: Delivery updated locally");
        OnDataChanged();

        if (await CanSyncWithServerAsync())
        {
            if (delivery.DeliveryId > 0)
            {
                try
                {
                    await _apiService.UpdateDeliveryAsync(delivery);
                    Debug.WriteLine($"DataService: Delivery updated on server");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DataService: Server update failed (will sync later): {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Verwijder een levering (soft delete) - lokaal EN op server indien online
    /// </summary>
    public async Task DeleteDeliveryAsync(Delivery delivery)
    {
        Debug.WriteLine($"DataService: Deleting delivery '{delivery.ReferenceNumber}' (ID: {delivery.DeliveryId})");

        delivery.IsDeleted = true;
        delivery.DeletedDate = DateTime.Now;
        _localContext.Deliveries.Update(delivery);
        await _localContext.SaveChangesAsync();
        Debug.WriteLine($"DataService: Delivery soft-deleted locally");
        OnDataChanged();

        if (await CanSyncWithServerAsync())
        {
            if (delivery.DeliveryId > 0)
            {
                try
                {
                    await _apiService.DeleteDeliveryAsync(delivery.DeliveryId);
                    Debug.WriteLine($"DataService: Delivery deleted on server");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DataService: Server delete failed (will sync later): {ex.Message}");
                }
            }
        }
    }

    #endregion
}
