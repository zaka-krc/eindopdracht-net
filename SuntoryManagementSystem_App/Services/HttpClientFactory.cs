namespace SuntoryManagementSystem_App.Services;

/// <summary>
/// Factory voor het aanmaken van geconfigureerde HttpClient instances.
/// Centraliseert SSL configuratie en timeout settings.
/// </summary>
public static class HttpClientFactory
{
    private static HttpClient? _sharedClient;
    private static readonly object _lock = new();
    
    /// <summary>
    /// Timeout in seconden voor HTTP requests
    /// </summary>
    public static int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Maakt een nieuwe HttpClientHandler met de juiste configuratie
    /// </summary>
    public static HttpClientHandler CreateHandler()
    {
        return new HttpClientHandler
        {
#if DEBUG
            // Voor development: SSL certificaat validatie uitschakelen
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
#endif
        };
    }
    
    /// <summary>
    /// Maakt een nieuwe HttpClient met de juiste configuratie
    /// </summary>
    public static HttpClient CreateClient()
    {
        return new HttpClient(CreateHandler())
        {
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds)
        };
    }
    
    /// <summary>
    /// Geeft een gedeelde HttpClient instance terug (singleton pattern)
    /// Gebruik dit voor de meeste API calls om resources te besparen
    /// </summary>
    public static HttpClient GetSharedClient()
    {
        if (_sharedClient == null)
        {
            lock (_lock)
            {
                _sharedClient ??= CreateClient();
            }
        }
        return _sharedClient;
    }
}
