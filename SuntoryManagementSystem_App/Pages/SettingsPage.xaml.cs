using SuntoryManagementSystem_App.Services;
using SuntoryManagementSystem_App.Services.Models;

namespace SuntoryManagementSystem_App.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly ConnectivityService _connectivityService;
    private readonly AuthService _authService;
    private readonly SyncService _syncService;
    
    public SettingsPage(ConnectivityService connectivityService, AuthService authService, SyncService syncService)
    {
        InitializeComponent();
        
        _connectivityService = connectivityService;
        _authService = authService;
        _syncService = syncService;
        
        // Subscribe to events
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
        _authService.AuthenticationChanged += OnAuthenticationChanged;
        _syncService.SyncStatusChanged += OnSyncStatusChanged;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        UpdateConnectivityUI(_connectivityService.IsConnected);
        await UpdateAuthUIAsync();
        UpdateSyncUI();
    }
    
    #region Connectivity
    
    private void OnConnectivityChanged(object? sender, bool isConnected)
    {
        MainThread.BeginInvokeOnMainThread(() => UpdateConnectivityUI(isConnected));
    }
    
    private void UpdateConnectivityUI(bool isConnected)
    {
        if (isConnected)
        {
            ConnectivityIndicator.TextColor = Colors.Green;
            ConnectivityLabel.Text = "Online";
            SyncButton.IsEnabled = true;
        }
        else
        {
            ConnectivityIndicator.TextColor = Colors.Red;
            ConnectivityLabel.Text = "Offline - Wijzigingen worden lokaal opgeslagen";
            SyncButton.IsEnabled = false;
        }
    }
    
    #endregion
    
    #region Authentication
    
    private void OnAuthenticationChanged(object? sender, bool isAuthenticated)
    {
        MainThread.BeginInvokeOnMainThread(async () => await UpdateAuthUIAsync());
    }
    
    private async Task UpdateAuthUIAsync()
    {
        var isAuthenticated = await _authService.IsAuthenticatedAsync();
        
        if (isAuthenticated)
        {
            LoginSection.IsVisible = false;
            LoggedInSection.IsVisible = true;
            
            var userName = await _authService.GetUserNameAsync();
            var email = await _authService.GetUserEmailAsync();
            
            UserNameLabel.Text = $"Ingelogd als: {userName ?? "Onbekend"}";
            UserEmailLabel.Text = $"Email: {email ?? "-"}";
        }
        else
        {
            LoginSection.IsVisible = true;
            LoggedInSection.IsVisible = false;
        }
    }
    
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            LoginErrorLabel.Text = "Vul e-mail en wachtwoord in";
            LoginErrorLabel.IsVisible = true;
            return;
        }
        
        LoginErrorLabel.IsVisible = false;
        
        var result = await _authService.LoginAsync(email, password);
        
        if (result.Success)
        {
            EmailEntry.Text = "";
            PasswordEntry.Text = "";
            
            await DisplayAlert("Succes", "Je bent ingelogd!", "OK");
            await UpdateAuthUIAsync();
        }
        else
        {
            LoginErrorLabel.Text = result.Message;
            LoginErrorLabel.IsVisible = true;
        }
    }
    
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Uitloggen", "Weet je zeker dat je wilt uitloggen?", "Ja", "Nee");
        
        if (confirm)
        {
            await _authService.LogoutAsync();
            await DisplayAlert("Uitgelogd", "Je bent uitgelogd.", "OK");
            await UpdateAuthUIAsync();
        }
    }
    
    #endregion
    
    #region Synchronization
    
    private void OnSyncStatusChanged(object? sender, SyncStatus status)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (status)
            {
                case SyncStatus.Syncing:
                    SyncIndicator.IsVisible = true;
                    SyncIndicator.IsRunning = true;
                    SyncButton.IsEnabled = false;
                    SyncStatusLabel.Text = "Bezig met synchroniseren...";
                    break;
                    
                case SyncStatus.Completed:
                    SyncIndicator.IsVisible = false;
                    SyncIndicator.IsRunning = false;
                    SyncButton.IsEnabled = _connectivityService.IsConnected;
                    SyncStatusLabel.Text = "Synchronisatie voltooid";
                    UpdateSyncUI();
                    break;
                    
                case SyncStatus.Failed:
                    SyncIndicator.IsVisible = false;
                    SyncIndicator.IsRunning = false;
                    SyncButton.IsEnabled = _connectivityService.IsConnected;
                    SyncStatusLabel.Text = "Synchronisatie mislukt";
                    break;
                    
                default:
                    SyncIndicator.IsVisible = false;
                    SyncIndicator.IsRunning = false;
                    SyncButton.IsEnabled = _connectivityService.IsConnected;
                    SyncStatusLabel.Text = "";
                    break;
            }
        });
    }
    
    private void UpdateSyncUI()
    {
        LastSyncLabel.Text = _syncService.LastSyncTime != DateTime.MinValue
            ? $"Laatst gesynchroniseerd: {_syncService.LastSyncTime:dd-MM-yyyy HH:mm}"
            : "Laatst gesynchroniseerd: Nooit";
    }
    
    private async void OnSyncClicked(object sender, EventArgs e)
    {
        if (!_connectivityService.IsConnected)
        {
            await DisplayAlert("Offline", "Je bent offline. Synchronisatie is niet mogelijk.", "OK");
            return;
        }
        
        var result = await _syncService.SyncAllAsync();
        
        var title = result.Success ? "Succes" : "Synchronisatie";
        var message = result.Success ? "Alle data is gesynchroniseerd met de server." : result.Message;
        
        await DisplayAlert(title, message, "OK");
    }
    
    #endregion
}
