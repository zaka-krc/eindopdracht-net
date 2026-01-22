using System.Net.Http.Json;
using System.Text.Json;
using SuntoryManagementSystem.Models;

namespace SuntoryManagementSystem_App.Services;

/// <summary>
/// Service om de REST API te consumeren.
/// Biedt CRUD operaties voor alle entiteiten via HTTP requests.
/// </summary>
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ConnectivityService _connectivityService;
    private readonly AuthService _authService;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public ApiService(ConnectivityService connectivityService, AuthService authService)
    {
        _connectivityService = connectivityService;
        _authService = authService;
        _httpClient = HttpClientFactory.CreateClient();
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    
    /// <summary>
    /// Voegt de authorization header toe aan de HttpClient
    /// </summary>
    private async Task SetAuthorizationHeaderAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
    
    /// <summary>
    /// Controleert connectiviteit en gooit een exception als offline
    /// </summary>
    private void EnsureConnected()
    {
        if (!_connectivityService.IsConnected)
            throw new InvalidOperationException("Geen internetverbinding");
    }
    
    #region Products
    
    public async Task<List<Product>> GetProductsAsync()
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.GetAsync(ApiSettings.ProductsEndpoint);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<Product>>(_jsonOptions) ?? [];
    }
    
    public async Task<Product?> GetProductAsync(int id)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.GetAsync($"{ApiSettings.ProductsEndpoint}/{id}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>(_jsonOptions);
    }
    
    public async Task<Product> CreateProductAsync(Product product)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.PostAsJsonAsync(ApiSettings.ProductsEndpoint, product, _jsonOptions);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"ApiService: CreateProduct failed - Status: {response.StatusCode}, Content: {errorContent}");
            response.EnsureSuccessStatusCode();
        }
        
        return await response.Content.ReadFromJsonAsync<Product>(_jsonOptions) ?? product;
    }
    
    public async Task UpdateProductAsync(Product product)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.PutAsJsonAsync($"{ApiSettings.ProductsEndpoint}/{product.ProductId}", product, _jsonOptions);
        response.EnsureSuccessStatusCode();
    }
    
    public async Task DeleteProductAsync(int id)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.DeleteAsync($"{ApiSettings.ProductsEndpoint}/{id}");
        response.EnsureSuccessStatusCode();
    }
    
    #endregion
    
    #region Customers
    
    public async Task<List<Customer>> GetCustomersAsync()
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.GetAsync(ApiSettings.CustomersEndpoint);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<Customer>>(_jsonOptions) ?? [];
    }
    
    public async Task<Customer?> GetCustomerAsync(int id)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.GetAsync($"{ApiSettings.CustomersEndpoint}/{id}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Customer>(_jsonOptions);
    }
    
    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.PostAsJsonAsync(ApiSettings.CustomersEndpoint, customer, _jsonOptions);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"ApiService: CreateCustomer failed - Status: {response.StatusCode}, Content: {errorContent}");
            response.EnsureSuccessStatusCode();
        }
        
        return await response.Content.ReadFromJsonAsync<Customer>(_jsonOptions) ?? customer;
    }
    
    public async Task UpdateCustomerAsync(Customer customer)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.PutAsJsonAsync($"{ApiSettings.CustomersEndpoint}/{customer.CustomerId}", customer, _jsonOptions);
        response.EnsureSuccessStatusCode();
    }
    
    public async Task DeleteCustomerAsync(int id)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.DeleteAsync($"{ApiSettings.CustomersEndpoint}/{id}");
        response.EnsureSuccessStatusCode();
    }
    
    #endregion
    
    #region Deliveries
    
    public async Task<List<Delivery>> GetDeliveriesAsync()
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.GetAsync(ApiSettings.DeliveriesEndpoint);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<Delivery>>(_jsonOptions) ?? [];
    }
    
    public async Task<Delivery?> GetDeliveryAsync(int id)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.GetAsync($"{ApiSettings.DeliveriesEndpoint}/{id}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Delivery>(_jsonOptions);
    }
    
    public async Task<Delivery> CreateDeliveryAsync(Delivery delivery)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.PostAsJsonAsync(ApiSettings.DeliveriesEndpoint, delivery, _jsonOptions);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"ApiService: CreateDelivery failed - Status: {response.StatusCode}, Content: {errorContent}");
            response.EnsureSuccessStatusCode();
        }
        
        return await response.Content.ReadFromJsonAsync<Delivery>(_jsonOptions) ?? delivery;
    }
    
    public async Task UpdateDeliveryAsync(Delivery delivery)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.PutAsJsonAsync($"{ApiSettings.DeliveriesEndpoint}/{delivery.DeliveryId}", delivery, _jsonOptions);
        response.EnsureSuccessStatusCode();
    }
    
    public async Task DeleteDeliveryAsync(int id)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.DeleteAsync($"{ApiSettings.DeliveriesEndpoint}/{id}");
        response.EnsureSuccessStatusCode();
    }
    
    #endregion
    
    #region Suppliers
    
    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.GetAsync(ApiSettings.SuppliersEndpoint);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<Supplier>>(_jsonOptions) ?? [];
    }
    
    public async Task<Supplier?> GetSupplierAsync(int id)
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.GetAsync($"{ApiSettings.SuppliersEndpoint}/{id}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Supplier>(_jsonOptions);
    }
    
    #endregion
    
    #region Vehicles
    
    public async Task<List<Vehicle>> GetVehiclesAsync()
    {
        EnsureConnected();
        await SetAuthorizationHeaderAsync();
        
        var response = await _httpClient.GetAsync(ApiSettings.VehiclesEndpoint);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<Vehicle>>(_jsonOptions) ?? [];
    }
    
    #endregion
}
