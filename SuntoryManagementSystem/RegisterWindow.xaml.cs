// ============================================================================
// SUNTORY MANAGEMENT SYSTEM (SMS)
// RegisterWindow.xaml.cs - Registratie Venster voor Nieuwe Gebruikers
// ============================================================================

using Microsoft.AspNetCore.Identity;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_Models.Data;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SuntoryManagementSystem
{
    public partial class RegisterWindow : Window
    {
        private readonly SuntoryDbContext _context;

        public ApplicationUser? NewUser { get; private set; }

        public RegisterWindow()
        {
            InitializeComponent();
            _context = new SuntoryDbContext();
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Reset error
                txtError.Visibility = Visibility.Collapsed;
                txtError.Text = string.Empty;

                // Validatie
                string fullName = txtFullName.Text.Trim();
                string email = txtEmail.Text.Trim().ToLower();
                string department = (cmbDepartment.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
                string jobTitle = txtJobTitle.Text.Trim();
                string password = txtPassword.Password;
                string confirmPassword = txtConfirmPassword.Password;

                System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: Naam='{fullName}', Email='{email}', Wachtwoord lengte={password.Length}");

                // Verplichte velden
                if (string.IsNullOrEmpty(fullName))
                {
                    ShowError("Volledige naam is verplicht.");
                    txtFullName.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(email))
                {
                    ShowError("E-mailadres is verplicht.");
                    txtEmail.Focus();
                    return;
                }

                // Email validatie
                if (!IsValidEmail(email))
                {
                    ShowError("Ongeldig e-mailadres formaat.");
                    txtEmail.Focus();
                    return;
                }

                // Check of email al bestaat
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == email);
                if (existingUser != null)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: Email '{email}' bestaat al in database!");
                    ShowError("Dit e-mailadres is al geregistreerd.");
                    txtEmail.Focus();
                    return;
                }

                // Wachtwoord validatie
                if (string.IsNullOrEmpty(password))
                {
                    ShowError("Wachtwoord is verplicht.");
                    txtPassword.Focus();
                    return;
                }

                if (password.Length < 6)
                {
                    ShowError("Wachtwoord moet minimaal 6 tekens lang zijn.");
                    txtPassword.Focus();
                    return;
                }

                if (!password.Any(char.IsUpper))
                {
                    ShowError("Wachtwoord moet minimaal 1 hoofdletter bevatten.");
                    txtPassword.Focus();
                    return;
                }

                if (!password.Any(char.IsDigit))
                {
                    ShowError("Wachtwoord moet minimaal 1 cijfer bevatten.");
                    txtPassword.Focus();
                    return;
                }

                if (password != confirmPassword)
                {
                    ShowError("Wachtwoorden komen niet overeen.");
                    txtConfirmPassword.Focus();
                    return;
                }

                // Maak nieuwe gebruiker aan
                var newUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = email,
                    NormalizedUserName = email.ToUpper(),
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    EmailConfirmed = true,
                    FullName = fullName,
                    Department = string.IsNullOrEmpty(department) ? null : department,
                    JobTitle = string.IsNullOrEmpty(jobTitle) ? null : jobTitle,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: Nieuwe gebruiker ID='{newUser.Id}', UserName='{newUser.UserName}', Email='{newUser.Email}'");

                // Hash wachtwoord
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                newUser.PasswordHash = passwordHasher.HashPassword(newUser, password);

                System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: Wachtwoord gehashed, hash lengte={newUser.PasswordHash?.Length ?? 0}");

                // Voeg gebruiker toe aan database
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: Gebruiker opgeslagen in database! Email in DB: '{newUser.Email}'");

                // ? NIEUWE CODE: Wijs automatisch EMPLOYEE rol toe aan nieuwe gebruikers
                var employeeRole = _context.Roles.FirstOrDefault(r => r.Name == "Employee");
                if (employeeRole != null)
                {
                    var userRole = new IdentityUserRole<string>
                    {
                        UserId = newUser.Id,
                        RoleId = employeeRole.Id
                    };
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: Employee rol toegewezen aan gebruiker '{newUser.FullName}'");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: WAARSCHUWING - Employee rol niet gevonden in database!");
                }

                // Verificatie met HUIDIGE context
                var verification1 = _context.Users.FirstOrDefault(u => u.Email == email);
                if (verification1 != null)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: VERIFICATIE 1 (huidige context) OK - Email='{verification1.Email}', IsActive={verification1.IsActive}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: VERIFICATIE 1 (huidige context) GEFAALD!");
                }

                // Verificatie met NIEUWE context (test of het echt in de database staat)
                using (var testContext = new SuntoryDbContext())
                {
                    var verification2 = testContext.Users.FirstOrDefault(u => u.Email == email);
                    if (verification2 != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: VERIFICATIE 2 (nieuwe context) OK - Email='{verification2.Email}', IsActive={verification2.IsActive}, HashedPassword={verification2.PasswordHash?.Substring(0, 20)}...");
                        
                        // TEST: Probeer nu direct te verifiëren of het wachtwoord klopt
                        var testHasher = new PasswordHasher<ApplicationUser>();
                        var testResult = testHasher.VerifyHashedPassword(verification2, verification2.PasswordHash!, password);
                        System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: Wachtwoord verificatie test: {testResult}");
                        
                        // Check of de rol is toegewezen
                        var userRoles = (from ur in testContext.UserRoles
                                        where ur.UserId == verification2.Id
                                        join r in testContext.Roles on ur.RoleId equals r.Id
                                        select r.Name).ToList();
                        System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: Toegewezen rollen: {(userRoles.Any() ? string.Join(", ", userRoles) : "GEEN ROLLEN")}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: VERIFICATIE 2 (nieuwe context) GEFAALD - Gebruiker NIET in database!");
                        MessageBox.Show(
                            "KRITIEKE FOUT!\n\n" +
                            "De gebruiker is niet opgeslagen in de database.\n" +
                            "Dit kan een database configuratie probleem zijn.",
                            "Database Fout",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                }

                // Success
                NewUser = newUser;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG REGISTER: EXCEPTION - {ex.Message}\n{ex.StackTrace}");
                ShowError($"Fout bij registratie: {ex.Message}");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
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
