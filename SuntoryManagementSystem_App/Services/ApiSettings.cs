namespace SuntoryManagementSystem_App.Services;

/// <summary>
/// API configuratie settings voor de MAUI app
/// </summary>
public static class ApiSettings
{
    // Base URL van de Web API - pas dit aan naar je eigen server
#if DEBUG
    // Voor Android emulator: 10.0.2.2 verwijst naar localhost van de host machine
    // Voor iOS simulator: localhost werkt direct
    // Voor fysieke apparaten: gebruik het IP adres van je ontwikkelmachine
    public static string BaseUrl => DeviceInfo.Platform == DevicePlatform.Android 
        ? "https://10.0.2.2:7001" 
        : "https://localhost:7001";
#else
    // Productie URL
    public static string BaseUrl => "https://your-production-api.com";
#endif

    // API Endpoints
    public static string ProductsEndpoint => $"{BaseUrl}/api/Products";
    public static string CustomersEndpoint => $"{BaseUrl}/api/Customers";
    public static string DeliveriesEndpoint => $"{BaseUrl}/api/Deliveries";
    public static string DeliveryItemsEndpoint => $"{BaseUrl}/api/DeliveryItems";
    public static string SuppliersEndpoint => $"{BaseUrl}/api/Suppliers";
    public static string VehiclesEndpoint => $"{BaseUrl}/api/Vehicles";
    public static string StockAdjustmentsEndpoint => $"{BaseUrl}/api/StockAdjustments";
    public static string StockAlertsEndpoint => $"{BaseUrl}/api/StockAlerts";
    
    // Identity API Endpoints
    public static string LoginEndpoint => $"{BaseUrl}/api/Auth/login";
    public static string RegisterEndpoint => $"{BaseUrl}/api/Auth/register";
    public static string RefreshTokenEndpoint => $"{BaseUrl}/api/Auth/refresh";
    public static string LogoutEndpoint => $"{BaseUrl}/api/Auth/logout";
}
