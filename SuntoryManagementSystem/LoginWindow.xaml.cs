// ============================================================================
// SUNTORY MANAGEMENT SYSTEM (SMS)
// LoginWindow.xaml.cs - Login Venster voor Gebruikersauthenticatie
// ============================================================================

using Microsoft.AspNetCore.Identity;
using SuntoryManagementSystem.Models;
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

            // Zet focus op password als email al is ingevuld
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

                // Validatie
                string email = txtEmail.Text.Trim();
                string password = txtPassword.Password;

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

                // Zoek gebruiker in database (LINQ Query Syntax)
                var user = (from u in _context.Users
                           where u.Email == email
                           select u).FirstOrDefault();

                if (user == null)
                {
                    ShowError("Ongeldig e-mailadres of wachtwoord.");
                    return;
                }

                // Check of account actief is
                if (!user.IsActive)
                {
                    ShowError("Dit account is gedeactiveerd. Neem contact op met de beheerder.");
                    return;
                }

                // Verifieer wachtwoord
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, password);

                if (result == PasswordVerificationResult.Failed)
                {
                    ShowError("Ongeldig e-mailadres of wachtwoord.");
                    return;
                }

                // Update laatste login datum
                user.LastLoginDate = DateTime.Now;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Haal gebruikersrollen op (LINQ Query Syntax)
                var userRoles = (from ur in _context.UserRoles
                                where ur.UserId == user.Id
                                join r in _context.Roles on ur.RoleId equals r.Id
                                select r.Name).ToList();

                // Login succesvol
                LoggedInUser = user;
                
                MessageBox.Show(
                    $"Welkom, {user.FullName}!\n\n" +
                    $"Rol(len): {string.Join(", ", userRoles)}\n" +
                    $"Afdeling: {user.Department ?? "Niet ingesteld"}\n" +
                    $"Laatste login: {user.LastLoginDate?.ToString("dd-MM-yyyy HH:mm") ?? "Eerste keer"}",
                    "Login Succesvol",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Sluit het login venster met DialogResult = true
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
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
