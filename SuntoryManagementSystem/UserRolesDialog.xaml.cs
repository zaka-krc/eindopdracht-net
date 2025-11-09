// ============================================================================
// SUNTORY MANAGEMENT SYSTEM (SMS)
// UserRolesDialog.xaml.cs - Dialog voor Rollenbeheer
// ============================================================================

using Microsoft.AspNetCore.Identity;
using SuntoryManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SuntoryManagementSystem
{
    public partial class UserRolesDialog : Window
    {
        private readonly SuntoryDbContext _context;
        private readonly ApplicationUser _user;
        private List<string>? _currentRoles;

        public UserRolesDialog(SuntoryDbContext context, ApplicationUser user)
        {
            InitializeComponent();
            _context = context;
            _user = user;

            // Set user info
            txtUserName.Text = user.FullName;
            txtUserEmail.Text = user.Email!;

            // Load current roles using LINQ Query Syntax
            LoadCurrentRoles();
        }

        private void LoadCurrentRoles()
        {
            try
            {
                // LINQ Query Syntax om huidige rollen van gebruiker op te halen
                _currentRoles = (from ur in _context.UserRoles
                                where ur.UserId == _user.Id
                                join r in _context.Roles on ur.RoleId equals r.Id
                                select r.Name ?? string.Empty).ToList();

                // Set checkboxes based on current roles
                chkAdministrator.IsChecked = _currentRoles.Contains("Administrator");
                chkManager.IsChecked = _currentRoles.Contains("Manager");
                chkEmployee.IsChecked = _currentRoles.Contains("Employee");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden van rollen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check welke rollen geselecteerd zijn
                var selectedRoles = new List<string>();
                
                if (chkAdministrator.IsChecked == true)
                    selectedRoles.Add("Administrator");
                if (chkManager.IsChecked == true)
                    selectedRoles.Add("Manager");
                if (chkEmployee.IsChecked == true)
                    selectedRoles.Add("Employee");

                // Minimaal 1 rol vereist
                if (!selectedRoles.Any())
                {
                    MessageBox.Show("Selecteer minimaal één rol!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Verwijder alle huidige rollen
                var currentUserRoles = _context.UserRoles.Where(ur => ur.UserId == _user.Id).ToList();
                _context.UserRoles.RemoveRange(currentUserRoles);

                // Voeg nieuwe rollen toe (LINQ Query Syntax)
                foreach (var roleName in selectedRoles)
                {
                    var role = (from r in _context.Roles
                               where r.Name == roleName
                               select r).FirstOrDefault();

                    if (role != null)
                    {
                        _context.UserRoles.Add(new IdentityUserRole<string>
                        {
                            UserId = _user.Id,
                            RoleId = role.Id
                        });
                    }
                }

                _context.SaveChanges();

                // Success
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan van rollen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
