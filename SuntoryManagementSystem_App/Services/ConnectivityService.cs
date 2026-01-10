namespace SuntoryManagementSystem_App.Services;

/// <summary>
/// Service om de netwerkconnectiviteit te controleren.
/// Detecteert of het apparaat online of offline is.
/// </summary>
public class ConnectivityService
{
    private readonly IConnectivity _connectivity;
    
    public ConnectivityService()
    {
        _connectivity = Connectivity.Current;
        _connectivity.ConnectivityChanged += OnConnectivityChanged;
    }
    
    /// <summary>
    /// Event dat wordt getriggerd wanneer de connectiviteit verandert
    /// </summary>
    public event EventHandler<bool>? ConnectivityChanged;
    
    /// <summary>
    /// Controleert of het apparaat verbonden is met internet
    /// </summary>
    public bool IsConnected => _connectivity.NetworkAccess == NetworkAccess.Internet;
    
    /// <summary>
    /// Controleert of het apparaat verbonden is via WiFi
    /// </summary>
    public bool IsWifiConnected => _connectivity.ConnectionProfiles.Contains(ConnectionProfile.WiFi);
    
    /// <summary>
    /// Controleert of het apparaat verbonden is via mobiele data
    /// </summary>
    public bool IsCellularConnected => _connectivity.ConnectionProfiles.Contains(ConnectionProfile.Cellular);
    
    /// <summary>
    /// Geeft het huidige type netwerkverbinding terug
    /// </summary>
    public NetworkAccess CurrentNetworkAccess => _connectivity.NetworkAccess;
    
    /// <summary>
    /// Geeft alle actieve verbindingsprofielen terug
    /// </summary>
    public IEnumerable<ConnectionProfile> ConnectionProfiles => _connectivity.ConnectionProfiles;
    
    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var isConnected = e.NetworkAccess == NetworkAccess.Internet;
        ConnectivityChanged?.Invoke(this, isConnected);
        
        System.Diagnostics.Debug.WriteLine($"Connectivity changed: {e.NetworkAccess}");
        System.Diagnostics.Debug.WriteLine($"Is connected: {isConnected}");
    }
    
    /// <summary>
    /// Controleert of een specifieke host bereikbaar is
    /// </summary>
    public async Task<bool> IsHostReachableAsync(string host, int timeoutMs = 5000)
    {
        if (!IsConnected) return false;
        
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeoutMs) };
            var response = await client.GetAsync(host);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Controleert of de API server bereikbaar is
    /// </summary>
    public async Task<bool> IsApiReachableAsync()
    {
        return await IsHostReachableAsync(ApiSettings.BaseUrl);
    }
}
