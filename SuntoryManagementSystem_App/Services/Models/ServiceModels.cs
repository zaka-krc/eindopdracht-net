namespace SuntoryManagementSystem_App.Services.Models;

/// <summary>
/// Request model voor login
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request model voor registratie
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Request model voor token refresh
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Response model voor authenticatie
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; } = 3600;
    public string? Email { get; set; }
    public string? UserName { get; set; }
}

/// <summary>
/// Resultaat van een authenticatie actie
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Resultaat van een synchronisatie actie
/// </summary>
public class SyncResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Status van de synchronisatie
/// </summary>
public enum SyncStatus
{
    Idle,
    Syncing,
    Completed,
    Failed
}
