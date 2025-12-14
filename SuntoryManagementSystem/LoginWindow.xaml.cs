// ============================================================================
// SUNTORY MANAGEMENT SYSTEM (SMS)
// LoginWindow.xaml.cs - Login Venster voor Gebruikersauthenticatie
// ============================================================================

using Microsoft.AspNetCore.Identity;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_Models.Data;
using System;
using System.Linq;
using System.Windows;

namespace SuntoryManagementSystem
{
    public partial class LoginWindow : Window
    {
        private readonly SuntoryDbContext _context;

        public ApplicationUser? LoggedInUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            _context = new SuntoryDbContext();
            
            // Zorg dat de database en seeding bestaan
            SuntoryDbContext.Seeder(_context);

            // Zet focus on password als email al is ingevuld
            if (!string.IsNullOrEmpty(txtEmail.Text))
            {
                txtPassword.Focus();
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Reset error message
                txtError.Visibility = Visibility.Collapsed;
                txtError.Text = string.Empty;

                // Validatie - converteer email naar lowercase voor consistente vergelijking
                string email = txtEmail.Text.Trim().ToLower();
                string password = txtPassword.Password;

                System.Diagnostics.Debug.WriteLine("????????????????????????????????????????");
                System.Diagnostics.Debug.WriteLine("DEBUG LOGIN: START LOGIN ATTEMPT");
                System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Email ingevoerd: '{email}'");
                System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Wachtwoord lengte: {password.Length}");

                if (string.IsNullOrEmpty(email))
                {
                    ShowError("Vul een e-mailadres in.");
                    txtEmail.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    ShowError("Vul een wachtwoord in.");
                    txtPassword.Focus();
                    return;
                }

                // DEBUG: Toon alle emails in de database voor debugging
                var allUsers = _context.Users.Select(u => new { u.Email, u.FullName, u.IsActive }).ToList();
                System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Totaal gebruikers in database: {allUsers.Count}");
                foreach (var u in allUsers)
                {
                    System.Diagnostics.Debug.WriteLine($"  - Email: '{u.Email}', Naam: '{u.FullName}', Actief: {u.IsActive}");
                }

                // Zoek gebruiker in database (LINQ Query Syntax) - gebruik lowercase email
                var user = (from u in _context.Users
                           where u.Email == email
                           select u).FirstOrDefault();

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: GEBRUIKER NIET GEVONDEN voor email: '{email}'");
                    System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Zoek opties:");
                    
                    // Probeer case-insensitive zoeken
                    var caseInsensitiveUser = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email);
                    if (caseInsensitiveUser != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Wel gevonden met case-insensitive: '{caseInsensitiveUser.Email}'");
                    }
                    
                    // Probeer op normalized email
                    var normalizedUser = _context.Users.FirstOrDefault(u => u.NormalizedEmail == email.ToUpper());
                    if (normalizedUser != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Wel gevonden met NormalizedEmail: '{normalizedUser.Email}'");
                    }
                    
                    ShowError("Ongeldig e-mailadres of wachtwoord.");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Gebruiker gevonden!");
                System.Diagnostics.Debug.WriteLine($"  - ID: {user.Id}");
                System.Diagnostics.Debug.WriteLine($"  - Naam: {user.FullName}");
                System.Diagnostics.Debug.WriteLine($"  - Email: '{user.Email}'");
                System.Diagnostics.Debug.WriteLine($"  - UserName: '{user.UserName}'");
                System.Diagnostics.Debug.WriteLine($"  - NormalizedEmail: '{user.NormalizedEmail}'");
                System.Diagnostics.Debug.WriteLine($"  - IsActive: {user.IsActive}");
                
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    int hashLength = user.PasswordHash.Length;
                    System.Diagnostics.Debug.WriteLine($"  - PasswordHash: {user.PasswordHash.Substring(0, Math.Min(30, hashLength))}...");
                }

                // Check of account actief is
                if (!user.IsActive)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Account is niet actief!");
                    ShowError("Dit account is gedeactiveerd. Neem contact op met de beheerder.");
                    return;
                }

                // Verifieer wachtwoord
                System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Wachtwoord verificatie starten...");
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, password);

                System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Wachtwoord verificatie resultaat: {result}");

                if (result == PasswordVerificationResult.Failed)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Wachtwoord verificatie GEFAALD!");
                    System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Ingevoerd wachtwoord lengte: {password.Length}");
                    
                    if (!string.IsNullOrEmpty(user.PasswordHash))
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Hash in database: {user.PasswordHash.Substring(0, 50)}...");
                    }
                    
                    // Test: Hash het ingevoerde wachtwoord en vergelijk
                    var testHash = passwordHasher.HashPassword(user, password);
                    System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Test hash van ingevoerd wachtwoord: {testHash.Substring(0, 50)}...");
                    
                    ShowError("Ongeldig e-mailadres of wachtwoord.");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Wachtwoord verificatie GESLAAGD!");

                // Update laatste login datum
                user.LastLoginDate = DateTime.Now;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Haal gebruikersrollen op (LINQ Query Syntax)
                var userRoles = (from ur in _context.UserRoles
                                where ur.UserId == user.Id
                                join r in _context.Roles on ur.RoleId equals r.Id
                                select r.Name).ToList();

                System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: Gebruikersrollen: {(userRoles.Any() ? string.Join(", ", userRoles) : "GEEN ROLLEN")}");
                System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: LOGIN SUCCESVOL");
                System.Diagnostics.Debug.WriteLine("????????????????????????????????????????");

                // Login succesvol
                LoggedInUser = user;
                
                string welcomeMessage = $"Welkom, {user.FullName}!\n\n";
                welcomeMessage += $"Rol(len): {(userRoles.Any() ? string.Join(", ", userRoles) : "Geen rollen toegewezen")}\n";
                welcomeMessage += $"Afdeling: {user.Department ?? "Niet ingesteld"}\n";
                welcomeMessage += $"Laatste login: {(user.LastLoginDate.HasValue ? user.LastLoginDate.Value.ToString("dd-MM-yyyy HH:mm") : "Eerste keer")}";
                
                MessageBox.Show(
                    welcomeMessage,
                    "Login Succesvol",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Sluit het login venster met DialogResult = true
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG LOGIN: EXCEPTION");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine("????????????????????????????????????????");
                ShowError($"Fout bij inloggen: {ex.Message}");
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            if (registerWindow.ShowDialog() == true)
            {
                MessageBox.Show(
                    "Registratie succesvol!\n\nU kunt nu inloggen met uw nieuwe account.",
                    "Registratie Voltooid",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Vul email in van nieuw account
                if (registerWindow.NewUser != null)
                {
                    txtEmail.Text = registerWindow.NewUser.Email!;
                    txtPassword.Focus();
                }
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}
