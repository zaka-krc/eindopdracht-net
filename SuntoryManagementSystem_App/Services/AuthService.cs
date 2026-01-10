using System.Net.Http.Json;
using System.Text.Json;
using SuntoryManagementSystem_App.Services.Models;

namespace SuntoryManagementSystem_App.Services;

/// <summary>
/// Service voor authenticatie via Identity API.
/// Beheert login, logout, token opslag en refresh.
/// </summary>
public class AuthService
{
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string TokenExpiryKey = "token_expiry";
    private const string UserEmailKey = "user_email";
    private const string UserNameKey = "user_name";
    
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public AuthService()
    {
        _httpClient = HttpClientFactory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    
    /// <summary>
    /// Event dat wordt getriggerd wanneer de authenticatiestatus verandert
    /// </summary>
    public event EventHandler<bool>? AuthenticationChanged;
    
    /// <summary>
    /// Controleert of de gebruiker is ingelogd
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;
            
        // Controleer of token niet verlopen is
        var expiryString = await SecureStorage.GetAsync(TokenExpiryKey);
        if (DateTime.TryParse(expiryString, out var expiry))
        {
            if (expiry <= DateTime.UtcNow)
            {
                // Token is verlopen, probeer te refreshen
                return await RefreshTokenAsync();
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Haalt de opgeslagen token op
    /// </summary>
    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(TokenKey);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Haalt de huidige gebruikersnaam op
    /// </summary>
    public async Task<string?> GetUserNameAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(UserNameKey);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Haalt het e-mailadres van de huidige gebruiker op
    /// </summary>
    public async Task<string?> GetUserEmailAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(UserEmailKey);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Logt in met email en wachtwoord
    /// </summary>
    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };
            
            var response = await _httpClient.PostAsJsonAsync(ApiSettings.LoginEndpoint, loginRequest, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
                
                if (authResponse != null)
                {
                    await SaveTokensAsync(authResponse);
                    AuthenticationChanged?.Invoke(this, true);
                    
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Login succesvol"
                    };
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResult
            {
                Success = false,
                Message = $"Login mislukt: {response.StatusCode} - {errorContent}"
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            return new AuthResult
            {
                Success = false,
                Message = $"Login fout: {ex.Message}"
            };
        }
    }
    
    /// <summary>
    /// Registreert een nieuwe gebruiker
    /// </summary>
    public async Task<AuthResult> RegisterAsync(string email, string password, string confirmPassword)
    {
        try
        {
            if (password != confirmPassword)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Wachtwoorden komen niet overeen"
                };
            }
            
            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };
            
            var response = await _httpClient.PostAsJsonAsync(ApiSettings.RegisterEndpoint, registerRequest, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
                
                if (authResponse != null)
                {
                    await SaveTokensAsync(authResponse);
                    AuthenticationChanged?.Invoke(this, true);
                    
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Registratie succesvol"
                    };
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResult
            {
                Success = false,
                Message = $"Registratie mislukt: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
            return new AuthResult
            {
                Success = false,
                Message = $"Registratie fout: {ex.Message}"
            };
        }
    }
    
    /// <summary>
    /// Vernieuwt de access token met de refresh token
    /// </summary>
    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
                return false;
            
            var refreshRequest = new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            };
            
            var response = await _httpClient.PostAsJsonAsync(ApiSettings.RefreshTokenEndpoint, refreshRequest, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
                
                if (authResponse != null)
                {
                    await SaveTokensAsync(authResponse);
                    return true;
                }
            }
            
            // Refresh gefaald, log uit
            await LogoutAsync();
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Refresh token error: {ex.Message}");
            await LogoutAsync();
            return false;
        }
    }
    
    /// <summary>
    /// Logt de gebruiker uit
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            // Probeer server-side logout
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    
                await _httpClient.PostAsync(ApiSettings.LogoutEndpoint, null);
            }
        }
        catch
        {
            // Negeer fouten bij server logout
        }
        finally
        {
            // Verwijder lokale tokens
            SecureStorage.Remove(TokenKey);
            SecureStorage.Remove(RefreshTokenKey);
            SecureStorage.Remove(TokenExpiryKey);
            SecureStorage.Remove(UserEmailKey);
            SecureStorage.Remove(UserNameKey);
            
            // Clear authorization header
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            AuthenticationChanged?.Invoke(this, false);
        }
    }
    
    private async Task SaveTokensAsync(AuthResponse response)
    {
        await SecureStorage.SetAsync(TokenKey, response.Token);
        
        if (!string.IsNullOrEmpty(response.RefreshToken))
            await SecureStorage.SetAsync(RefreshTokenKey, response.RefreshToken);
            
        var expiry = DateTime.UtcNow.AddSeconds(response.ExpiresIn);
        await SecureStorage.SetAsync(TokenExpiryKey, expiry.ToString("O"));
        
        if (!string.IsNullOrEmpty(response.Email))
            await SecureStorage.SetAsync(UserEmailKey, response.Email);
            
        if (!string.IsNullOrEmpty(response.UserName))
            await SecureStorage.SetAsync(UserNameKey, response.UserName);
    }
}
