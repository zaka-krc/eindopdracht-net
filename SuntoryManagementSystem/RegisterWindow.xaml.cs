// ============================================================================
// SUNTORY MANAGEMENT SYSTEM (SMS)
// RegisterWindow.xaml.cs - Registratie Venster voor Nieuwe Gebruikers
// ============================================================================

using Microsoft.AspNetCore.Identity;
using SuntoryManagementSystem.Models;
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

                // Hash wachtwoord
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                newUser.PasswordHash = passwordHasher.HashPassword(newUser, password);

                // Voeg gebruiker toe aan database
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // GEEN ROL TOEWIJZEN - Gebruikers krijgen standaard Guest rechten
                // Administrator kan later rollen toewijzen via User Management

                // Success
                NewUser = newUser;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
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
