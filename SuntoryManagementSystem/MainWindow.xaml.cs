using SuntoryManagementSystem.Models;
using SuntoryManagementSystem.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SuntoryManagementSystem_Models.Data;

namespace SuntoryManagementSystem
{
    public partial class MainWindow : Window
    {
        private SuntoryDbContext _context;
        private ApplicationUser _currentUser;
        private List<string> _userRoles;

        // Constructor ZONDER parameters - Start in GUEST MODE
        public MainWindow() : this(null, new List<string>())
        {
        }

        // Nieuwe constructor MET ingelogde gebruiker en rollen
        public MainWindow(ApplicationUser? currentUser, List<string> userRoles)
        {
            InitializeComponent();
            
            _context = new SuntoryDbContext();
            SetCurrentUser(currentUser, userRoles);

            SuntoryDbContext.Seeder(_context);
            
            // Load data asynchronously
            Loaded += async (s, e) => await LoadAllDataAsync();
        }

        private void SetCurrentUser(ApplicationUser? user, List<string>? roles)
        {
            _currentUser = user ?? new ApplicationUser 
            { 
                FullName = "Gast", 
                Email = "guest@suntory.com",
                Id = "guest"
            };
            _userRoles = roles ?? new List<string> { "Guest" };

            UpdateUI();
        }

        private void UpdateUI()
        {
            // Configure buttons and user info based on login status
            bool isGuest = _userRoles.Contains("Guest") || _userRoles.Count == 0 || _currentUser.Id == "guest";
            
            if (isGuest)
            {
                // GUEST MODE: Hide user info, show login & register buttons
                btnUserInfo.Visibility = Visibility.Collapsed;
                btnLogin.Visibility = Visibility.Visible;
                btnRegister.Visibility = Visibility.Visible;
                btnLogout.Visibility = Visibility.Collapsed;
            }
            else
            {
                // LOGGED IN: Show compact user button and logout button
                btnUserInfo.Visibility = Visibility.Visible;
                txtUserNameCompact.Text = _currentUser.FullName;
                txtUserRoleCompact.Text = string.Join(", ", _userRoles);
                
                // Set first letter of name as initial
                txtUserInitial.Text = !string.IsNullOrEmpty(_currentUser.FullName) 
                    ? _currentUser.FullName[0].ToString().ToUpper() 
                    : "U";

                btnLogin.Visibility = Visibility.Collapsed;
                btnRegister.Visibility = Visibility.Collapsed;
                btnLogout.Visibility = Visibility.Visible;
            }

            // Configure menu based on roles
            ConfigureMenuForRoles();
        }

        private void btnUserInfo_Click(object sender, RoutedEventArgs e)
        {
            // Show user info popup near the button
            var popup = new UserInfoPopup(_currentUser, string.Join(", ", _userRoles));
            popup.ShowNearElement(btnUserInfo);
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Open register window
            var registerWindow = new RegisterWindow();
            if (registerWindow.ShowDialog() == true && registerWindow.NewUser != null)
            {
                // FIXED: Dispose oude context en maak nieuwe
                _context?.Dispose();
                _context = new SuntoryDbContext();
                
                // Haal de rollen op van de nieuwe gebruiker
                var userRoles = (from ur in _context.UserRoles
                                where ur.UserId == registerWindow.NewUser.Id
                                join r in _context.Roles on ur.RoleId equals r.Id
                                select r.Name).ToList();

                MessageBox.Show(
                    $"Welkom, {registerWindow.NewUser.FullName}!\n\n" +
                    "Uw account is aangemaakt.\n" +
                    $"Uw rol: {(userRoles.Any() ? string.Join(", ", userRoles) : "Geen rol")}.\n\n" +
                    "Neem contact op met een administrator voor extra rechten.",
                    "Registratie Succesvol",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // FIXED: Update current window with correct roles
                SetCurrentUser(registerWindow.NewUser, userRoles);
                await LoadAllDataAsync();
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Open login window
            var loginWindow = new LoginWindow();
            
            if (loginWindow.ShowDialog() == true && loginWindow.LoggedInUser != null)
            {
                // FIXED: Dispose oude context en maak nieuwe
                _context?.Dispose();
                _context = new SuntoryDbContext();
                
                // Haal de rollen op van de ingelogde gebruiker
                var userRoles = (from ur in _context.UserRoles
                                where ur.UserId == loginWindow.LoggedInUser.Id
                                join r in _context.Roles on ur.RoleId equals r.Id
                                select r.Name).ToList();

                // FIXED: Update current window instead of creating new window
                SetCurrentUser(loginWindow.LoggedInUser, userRoles);
                await LoadAllDataAsync();
            }
        }

        private async void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Weet u zeker dat u wilt uitloggen, {_currentUser.FullName}?\n\n" +
                "U keert terug naar de alleen-lezen modus.",
                "Uitloggen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // FIXED: Dispose oude context en maak nieuwe
                _context?.Dispose();
                _context = new SuntoryDbContext();
                
                // Reset to guest mode in current window
                SetCurrentUser(null, new List<string> { "Guest" });
                await LoadAllDataAsync();
                
                MessageBox.Show(
                    "U bent uitgelogd.\n\nU kunt nu alleen-lezen gegevens bekijken.",
                    "Uitgelogd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ConfigureMenuForRoles()
        {
            // LINQ Query Syntax om te checken of gebruiker Administrator is
            bool isAdmin = (from role in _userRoles
                           where role == "Administrator"
                           select role).Any();

            bool isManager = _userRoles.Contains("Manager");
            bool isEmployee = _userRoles.Contains("Employee");
            // Guest is alleen wanneer expliciet Guest rol OF geen enkele rol EN niet ingelogd
            bool isGuest = (_userRoles.Contains("Guest") || _userRoles.Count == 0) && _currentUser.Id == "guest";

            // Reset all visibility first (toon alles standaard)
            ResetAllButtonVisibility();

            // ADMINISTRATOR: Volledige toegang + User Management
            if (isAdmin)
            {
                tabUserManagement.Visibility = Visibility.Visible;
                // Admin heeft toegang tot ALLES
                return;
            }

            // MANAGER: Volledige operationele toegang, GEEN User Management
            // Manager kan alles behalve gebruikers beheren
            if (isManager)
            {
                tabUserManagement.Visibility = Visibility.Collapsed;
                // Manager heeft volledige toegang tot alle operationele functies
                // Inclusief: toevoegen, wijzigen, verwijderen, rapporten genereren
                return;
            }

            // EMPLOYEE: Alleen-lezen toegang + Rapporten genereren
            // Employee kan ALLEEN data bekijken en rapporten maken
            if (isEmployee)
            {
                tabUserManagement.Visibility = Visibility.Collapsed;
                
                // Verberg ALLE wijzigingsknoppen
                // Suppliers
                btnAddSupplier.Visibility = Visibility.Collapsed;
                btnEditSupplier.Visibility = Visibility.Collapsed;
                btnDeleteSupplier.Visibility = Visibility.Collapsed;
                
                // Customers
                btnAddCustomer.Visibility = Visibility.Collapsed;
                btnEditCustomer.Visibility = Visibility.Collapsed;
                btnDeleteCustomer.Visibility = Visibility.Collapsed;
                
                // Products
                btnAddProduct.Visibility = Visibility.Collapsed;
                btnEditProduct.Visibility = Visibility.Collapsed;
                btnDeleteProduct.Visibility = Visibility.Collapsed;
                
                // Deliveries
                btnAddDelivery.Visibility = Visibility.Collapsed;
                btnEditDelivery.Visibility = Visibility.Collapsed;
                btnDeleteDelivery.Visibility = Visibility.Collapsed;
                btnProcessDelivery.Visibility = Visibility.Collapsed;
                
                // Vehicles
                btnAddVehicle.Visibility = Visibility.Collapsed;
                btnEditVehicle.Visibility = Visibility.Collapsed;
                btnDeleteVehicle.Visibility = Visibility.Collapsed;
                
                // Employee KAN WEL rapporten genereren
                btnGenerateReports.IsEnabled = true;
                btnGenerateReports.Opacity = 1.0;
                btnGenerateReports.ToolTip = null;
                
                return;
            }

            // GUEST MODE: Alleen-lezen toegang (data zichtbaar, niks aanpasbaar, geen rapporten)
            if (isGuest)
            {
                // Verberg user management
                tabUserManagement.Visibility = Visibility.Collapsed;
                
                // Verberg alle wijzigingsknoppen
                // Suppliers
                btnAddSupplier.Visibility = Visibility.Collapsed;
                btnEditSupplier.Visibility = Visibility.Collapsed;
                btnDeleteSupplier.Visibility = Visibility.Collapsed;
                
                // Customers
                btnAddCustomer.Visibility = Visibility.Collapsed;
                btnEditCustomer.Visibility = Visibility.Collapsed;
                btnDeleteCustomer.Visibility = Visibility.Collapsed;
                
                // Products
                btnAddProduct.Visibility = Visibility.Collapsed;
                btnEditProduct.Visibility = Visibility.Collapsed;
                btnDeleteProduct.Visibility = Visibility.Collapsed;
                
                // Deliveries
                btnAddDelivery.Visibility = Visibility.Collapsed;
                btnEditDelivery.Visibility = Visibility.Collapsed;
                btnDeleteDelivery.Visibility = Visibility.Collapsed;
                btnProcessDelivery.Visibility = Visibility.Collapsed;
                
                // Vehicles
                btnAddVehicle.Visibility = Visibility.Collapsed;
                btnEditVehicle.Visibility = Visibility.Collapsed;
                btnDeleteVehicle.Visibility = Visibility.Collapsed;
                
                // GUEST MAG GEEN RAPPORTEN GENEREREN
                btnGenerateReports.IsEnabled = false;
                btnGenerateReports.Opacity = 0.5;
                btnGenerateReports.ToolTip = "Rapporten genereren is alleen beschikbaar voor ingelogde gebruikers";
                
                return;
            }

            // DEFAULT VOOR INGELOGDE GEBRUIKERS ZONDER ROL: Behandel als Employee (alleen-lezen + rapporten)
            tabUserManagement.Visibility = Visibility.Collapsed;
            
            // Verberg ALLE wijzigingsknoppen
            btnAddSupplier.Visibility = Visibility.Collapsed;
            btnEditSupplier.Visibility = Visibility.Collapsed;
            btnDeleteSupplier.Visibility = Visibility.Collapsed;
            btnAddCustomer.Visibility = Visibility.Collapsed;
            btnEditCustomer.Visibility = Visibility.Collapsed;
            btnDeleteCustomer.Visibility = Visibility.Collapsed;
            btnAddProduct.Visibility = Visibility.Collapsed;
            btnEditProduct.Visibility = Visibility.Collapsed;
            btnDeleteProduct.Visibility = Visibility.Collapsed;
            btnAddDelivery.Visibility = Visibility.Collapsed;
            btnEditDelivery.Visibility = Visibility.Collapsed;
            btnDeleteDelivery.Visibility = Visibility.Collapsed;
            btnProcessDelivery.Visibility = Visibility.Collapsed;
            btnAddVehicle.Visibility = Visibility.Collapsed;
            btnEditVehicle.Visibility = Visibility.Collapsed;
            btnDeleteVehicle.Visibility = Visibility.Collapsed;
            
            // KAN WEL rapporten genereren
            btnGenerateReports.IsEnabled = true;
            btnGenerateReports.Opacity = 1.0;
            btnGenerateReports.ToolTip = null;
        }

        private void ResetAllButtonVisibility()
        {
            // Reset alle buttons en tabs naar standaard (zichtbaar/enabled)
            tabUserManagement.Visibility = Visibility.Visible;
            tabVehicles.Visibility = Visibility.Visible;
            
            // Suppliers
            btnAddSupplier.Visibility = Visibility.Visible;
            btnEditSupplier.Visibility = Visibility.Visible;
            btnDeleteSupplier.Visibility = Visibility.Visible;
            
            // Customers
            btnAddCustomer.Visibility = Visibility.Visible;
            btnEditCustomer.Visibility = Visibility.Visible;
            btnDeleteCustomer.Visibility = Visibility.Visible;
            
            // Products
            btnAddProduct.Visibility = Visibility.Visible;
            btnEditProduct.Visibility = Visibility.Visible;
            btnDeleteProduct.Visibility = Visibility.Visible;
            
            // Deliveries
            btnAddDelivery.Visibility = Visibility.Visible;
            btnEditDelivery.Visibility = Visibility.Visible;
            btnDeleteDelivery.Visibility = Visibility.Visible;
            btnProcessDelivery.Visibility = Visibility.Visible;
            
            // Vehicles
            btnAddVehicle.Visibility = Visibility.Visible;
            btnEditVehicle.Visibility = Visibility.Visible;
            btnDeleteVehicle.Visibility = Visibility.Visible;
            
            // Reports
            btnGenerateReports.IsEnabled = true;
            btnGenerateReports.Opacity = 1.0;
            btnGenerateReports.ToolTip = null;
        }
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
            Application.Current.Shutdown();
        }

        private async Task LoadSuppliersAsync()
        {
            dgSuppliers.ItemsSource = await _context.Suppliers
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Status == StatusConstants.Active ? 0 : 1) // Actieve eerst
                .ThenBy(s => s.SupplierName)
                .ToListAsync();
        }

        private async Task LoadCustomersAsync()
        {
            dgCustomers.ItemsSource = await _context.Customers
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Status == StatusConstants.Active ? 0 : 1) // Actieve eerst
                .ThenBy(c => c.CustomerName)
                .ToListAsync();
        }

        private async Task LoadProductsAsync()
        {
            dgProducts.ItemsSource = await _context.Products
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        private async Task LoadDeliveriesAsync()
        {
            dgDeliveries.ItemsSource = await _context.Deliveries
                .Include(d => d.Supplier)
                .Include(d => d.Customer)
                .Include(d => d.Vehicle)
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.ExpectedDeliveryDate)
                .ToListAsync();
        }

        private async Task LoadVehiclesAsync()
        {
            dgVehicles.ItemsSource = await _context.Vehicles
                .Where(v => !v.IsDeleted)
                .OrderBy(v => v.LicensePlate)
                .ToListAsync();
        }

        private async Task LoadLowStockProductsAsync()
        {
            // Gewoon producten tonen waar voorraad onder minimum is
            dgStockAlerts.ItemsSource = await _context.Products
                .Include(p => p.Supplier)
                .Where(p => !p.IsDeleted && p.IsActive)
                .Where(p => p.StockQuantity < p.MinimumStock)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        private async Task LoadStockAdjustmentsAsync()
        {
            dgStockAdjustments.ItemsSource = await _context.StockAdjustments
                .Include(sa => sa.Product)  // Laad Product relatie
                .Where(sa => !sa.IsDeleted)
                .OrderByDescending(sa => sa.AdjustmentDate)
                .ToListAsync();
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                // LINQ Query Syntax om alle gebruikers te laden
                var users = await (from u in _context.Users
                            orderby u.IsActive descending, u.FullName
                            select u).ToListAsync();

                dgUsers.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden van gebruikers: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAllDataAsync()
        {
            try
            {
                await LoadSuppliersAsync();
                await LoadCustomersAsync();
                await LoadProductsAsync();
                await LoadDeliveriesAsync();
                await LoadVehiclesAsync();
                await LoadLowStockProductsAsync();
                await LoadStockAdjustmentsAsync();
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden van data: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcMain.SelectedIndex == 4)
                await LoadLowStockProductsAsync(); // Refresh wanneer tab wordt geopend
            else if (tcMain.SelectedIndex == 5)
                await LoadStockAdjustmentsAsync();
        }

        private void dgSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnEditSupplier.IsEnabled = dgSuppliers.SelectedItem != null;
            btnDeleteSupplier.IsEnabled = dgSuppliers.SelectedItem != null;
        }

        private async void btnAddSupplier_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SupplierDialog();
            if (dialog.ShowDialog() == true)
            {
                _context.Suppliers.Add(dialog.Supplier);
                await _context.SaveChangesAsync();
                await LoadSuppliersAsync();
                MessageBox.Show("Leverancier succesvol toegevoegd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void btnEditSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (dgSuppliers.SelectedItem is Supplier selectedSupplier)
            {
                var dialog = new SupplierDialog(selectedSupplier);
                if (dialog.ShowDialog() == true)
                {
                    _context.Suppliers.Update(selectedSupplier);
                    await _context.SaveChangesAsync();
                    await LoadSuppliersAsync();
                    MessageBox.Show("Leverancier succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void btnDeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (dgSuppliers.SelectedItem is Supplier selectedSupplier)
            {
                var result = MessageBox.Show(
                    $"Weet u zeker dat u leverancier '{selectedSupplier.SupplierName}' wilt verwijderen?",
                    "Leverancier Verwijderen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // SOFT DELETE
                        selectedSupplier.IsDeleted = true;
                        selectedSupplier.DeletedDate = DateTime.Now;
                        _context.Suppliers.Update(selectedSupplier);
                        await _context.SaveChangesAsync();
                        await LoadSuppliersAsync();
                        MessageBox.Show("Leverancier succesvol verwijderd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Fout bij het verwijderen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void dgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnEditCustomer.IsEnabled = dgCustomers.SelectedItem != null;
            btnDeleteCustomer.IsEnabled = dgCustomers.SelectedItem != null;
        }

        private async void btnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CustomerDialog();
            if (dialog.ShowDialog() == true)
            {
                _context.Customers.Add(dialog.Customer);
                await _context.SaveChangesAsync();
                await LoadCustomersAsync();
                MessageBox.Show("Klant succesvol toegevoegd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void btnEditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer selectedCustomer)
            {
                var dialog = new CustomerDialog(selectedCustomer);
                if (dialog.ShowDialog() == true)
                {
                    _context.Customers.Update(selectedCustomer);
                    await _context.SaveChangesAsync();
                    await LoadCustomersAsync();
                    MessageBox.Show("Klant succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void btnDeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer selectedCustomer)
            {
                var result = MessageBox.Show(
                    $"Weet u zeker dat u klant '{selectedCustomer.CustomerName}' wilt verwijderen?",
                    "Klant Verwijderen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // SOFT DELETE
                        selectedCustomer.IsDeleted = true;
                        selectedCustomer.DeletedDate = DateTime.Now;
                        _context.Customers.Update(selectedCustomer);
                        await _context.SaveChangesAsync();
                        await LoadCustomersAsync();
                        MessageBox.Show("Klant succesvol verwijderd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Fout bij het verwijderen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void dgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnEditProduct.IsEnabled = dgProducts.SelectedItem != null;
            btnDeleteProduct.IsEnabled = dgProducts.SelectedItem != null;
        }

        private async void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductDialog(_context);
            if (dialog.ShowDialog() == true)
            {
                _context.Products.Add(dialog.Product);
                await _context.SaveChangesAsync();

                if (dialog.HasStockAdjustment)
                {
                    CreateStockAdjustmentForNewProduct(dialog.Product, dialog.InitialStock);
                    await _context.SaveChangesAsync();
                }

                await LoadProductsAsync();
                await LoadLowStockProductsAsync();
                await LoadStockAdjustmentsAsync();

                if (dialog.HasStockAdjustment)
                {
                    var details = BuildStockAdjustmentDetails(dialog.Product, 0, dialog.InitialStock, dialog.AdjustmentType, true);
                    MessageBox.Show(
                        details,
                        "Nieuw Product - Voorraad Details",
                        MessageBoxButton.OK,
                        dialog.Product.StockQuantity < dialog.Product.MinimumStock 
                            ? MessageBoxImage.Warning 
                            : MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Product succesvol toegevoegd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void btnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product selectedProduct)
            {
                int originalStock = selectedProduct.StockQuantity;
                
                var dialog = new ProductDialog(_context, selectedProduct);
                if (dialog.ShowDialog() == true)
                {
                    _context.Products.Update(selectedProduct);
                    await _context.SaveChangesAsync();

                    if (dialog.HasStockAdjustment)
                    {
                        CreateStockAdjustment(selectedProduct, originalStock, dialog.InitialStock, dialog.AdjustmentType);
                        await _context.SaveChangesAsync();
                    }

                    await LoadProductsAsync();
                    await LoadLowStockProductsAsync();
                    await LoadStockAdjustmentsAsync();

                    if (dialog.HasStockAdjustment)
                    {
                        var details = BuildStockAdjustmentDetails(selectedProduct, originalStock, dialog.InitialStock, dialog.AdjustmentType, false);
                        MessageBox.Show(
                            details,
                            "Voorraad Aanpassing Details",
                            MessageBoxButton.OK,
                            selectedProduct.StockQuantity < selectedProduct.MinimumStock
                                ? MessageBoxImage.Warning
                                : MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Product succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private async void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show(
                    $"Weet u zeker dat u product '{selectedProduct.ProductName}' wilt verwijderen?",
                    "Product Verwijderen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        selectedProduct.IsDeleted = true;
                        selectedProduct.DeletedDate = DateTime.Now;
                        _context.Products.Update(selectedProduct);
                        await _context.SaveChangesAsync();
                        await LoadProductsAsync();
                        MessageBox.Show("Product succesvol verwijderd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Fout bij het verwijderen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CreateStockAdjustmentForNewProduct(Product product, int initialStock)
        {
            var adjustment = new StockAdjustment
            {
                ProductId = product.ProductId,
                AdjustmentType = "Addition",
                QuantityChange = initialStock,
                PreviousQuantity = 0,
                NewQuantity = initialStock,
                Reason = $"Nieuw product aangemaakt met initiële voorraad van {initialStock} stuks",
                AdjustedBy = _currentUser.FullName,  // Gebruik ingelogde gebruiker
                AdjustmentDate = DateTime.Now
            };

            _context.StockAdjustments.Add(adjustment);

            if (initialStock < product.MinimumStock)
            {
                CreateOrUpdateStockAlert(product, initialStock);
            }
        }

        private void CreateStockAdjustment(Product product, int previousQuantity, int newQuantity, string adjustmentType)
        {
            int quantityChange = newQuantity - previousQuantity;
            string reason;

            if (string.IsNullOrEmpty(adjustmentType))
            {
                adjustmentType = quantityChange > 0 ? "Addition" : "Removal";
            }

            if (quantityChange > 0)
            {
                reason = adjustmentType switch
                {
                    "Correction" => $"Voorraad correctie na inventarisatie: +{quantityChange} stuks bijgeteld",
                    _ => $"Handmatige voorraad toevoeging: +{quantityChange} stuks toegevoegd"
                };
            }
            else
            {
                reason = adjustmentType switch
                {
                    "Damage" => $"Voorraad verminderd door schade: {quantityChange} stuks beschadigd",
                    "Theft" => $"Voorraad verminderd door diefstal: {quantityChange} stuks gestolen/vermist",
                    "Correction" => $"Voorraad correctie na inventarisatie: {quantityChange} stuks",
                    _ => $"Handmatige voorraad verwijdering: {quantityChange} stuks verwijderd"
                };
            }

            var adjustment = new StockAdjustment
            {
                ProductId = product.ProductId,
                AdjustmentType = adjustmentType,
                QuantityChange = quantityChange,
                PreviousQuantity = previousQuantity,
                NewQuantity = newQuantity,
                Reason = reason,
                AdjustedBy = _currentUser.FullName,  // Gebruik ingelogde gebruiker
                AdjustmentDate = DateTime.Now
            };

            _context.StockAdjustments.Add(adjustment);

            if (newQuantity < product.MinimumStock)
            {
                CreateOrUpdateStockAlert(product, newQuantity);
            }
            else if (previousQuantity < product.MinimumStock)
            {
                ResolveStockAlerts(product);
            }
        }

        private void CreateOrUpdateStockAlert(Product product, int currentStock)
        {
            var existingAlert = _context.StockAlerts
                .FirstOrDefault(sa => sa.ProductId == product.ProductId 
                    && sa.Status == "Active" 
                    && !sa.IsDeleted);

            if (existingAlert == null)
            {
                string alertType = currentStock == 0 ? "Out of Stock" : 
                                 currentStock < (product.MinimumStock / 2) ? "Critical" : 
                                 "Low Stock";

                var alert = new StockAlert
                {
                    ProductId = product.ProductId,
                    AlertType = alertType,
                    Status = "Active",
                    CreatedDate = DateTime.Now,
                    Notes = $"Voorraad is {currentStock} stuks, minimum is {product.MinimumStock}"
                };

                _context.StockAlerts.Add(alert);
            }
            else
            {
                existingAlert.AlertType = currentStock == 0 ? "Out of Stock" : 
                                        currentStock < (product.MinimumStock / 2) ? "Critical" : 
                                        "Low Stock";
                existingAlert.Notes = $"Voorraad is {currentStock} stuks, minimum is {product.MinimumStock}";
            }
        }

        private void ResolveStockAlerts(Product product)
        {
            var activeAlerts = _context.StockAlerts
                .Where(sa => sa.ProductId == product.ProductId 
                    && sa.Status == "Active" 
                    && !sa.IsDeleted)
                .ToList();

            foreach (var alert in activeAlerts)
            {
                alert.Status = "Resolved";
                alert.ResolvedDate = DateTime.Now;
                alert.Notes += " - Opgelost: voorraad weer boven minimum";
            }
        }

        private string BuildStockAdjustmentDetails(Product product, int previousQty, int newQty, string adjustmentType, bool isNewProduct)
        {
            var details = new System.Text.StringBuilder();
            details.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            details.AppendLine(isNewProduct ? "NIEUW PRODUCT AANGEMAAKT" : "VOORRAAD AANPASSING GEREGISTREERD");
            details.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            details.AppendLine();
            details.AppendLine($"Product: {product.ProductName}");
            details.AppendLine($"SKU: {product.SKU}");
            details.AppendLine();
            
            if (isNewProduct)
            {
                details.AppendLine("INITIËLE VOORRAAD:");
                details.AppendLine($"   Voorraad: {newQty} stuks");
                details.AppendLine($"   Minimum: {product.MinimumStock} stuks");
            }
            else
            {
                int quantityChange = newQty - previousQty;
                string changeSymbol = quantityChange > 0 ? "+" : "";
                details.AppendLine("VOORRAAD WIJZIGING:");
                details.AppendLine($"   Vorige voorraad: {previousQty} stuks");
                details.AppendLine($"   Wijziging: {changeSymbol}{quantityChange} stuks");
                details.AppendLine($"   Nieuwe voorraad: {newQty} stuks");
            }
            
            details.AppendLine();
            details.AppendLine($"Type: {adjustmentType}");
            details.AppendLine($"Datum/Tijd: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
            details.AppendLine();

            if (newQty < product.MinimumStock)
            {
                string alertType = newQty == 0 ? "Out of Stock" : 
                                 newQty < (product.MinimumStock / 2) ? "Critical" : "Low Stock";
                details.AppendLine("WAARSCHUWING:");
                details.AppendLine($"   Voorraad is onder minimum! ({product.MinimumStock})");
                details.AppendLine($"   Alert type: {alertType}");
                details.AppendLine("   > Voorraad waarschuwing aangemaakt");
            }
            else if (!isNewProduct && previousQty < product.MinimumStock && newQty >= product.MinimumStock)
            {
                details.AppendLine("VOORRAAD HERSTELD:");
                details.AppendLine($"   Voorraad is weer boven minimum! ({product.MinimumStock})");
                details.AppendLine("   > Actieve waarschuwingen opgelost");
            }
            else
            {
                details.AppendLine("Status: Voorraad binnen normale parameters");
            }

            details.AppendLine();
            details.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            details.AppendLine(isNewProduct ? "Product en voorraad succesvol aangemaakt!" : "De aanpassing is succesvol geregistreerd!");
            details.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            return details.ToString();
        }

        private void dgDeliveries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgDeliveries.SelectedItem is Delivery delivery)
            {
                bool isNotProcessed = !delivery.IsProcessed;
                bool isGepland = delivery.Status == "Gepland";
                bool isGeannuleerd = delivery.Status == "Geannuleerd";

                // Alleen Gepland en niet-verwerkte leveringen kunnen worden bewerkt
                btnEditDelivery.IsEnabled = isNotProcessed && !isGeannuleerd;

                // Leveringen kunnen altijd worden verwijderd (soft delete)
                btnDeleteDelivery.IsEnabled = true;

                // Alleen Gepland en niet-verwerkte leveringen kunnen worden verwerkt
                btnProcessDelivery.IsEnabled = isNotProcessed && isGepland;
            }
            else
            {
                btnEditDelivery.IsEnabled = false;
                btnDeleteDelivery.IsEnabled = false;
                btnProcessDelivery.IsEnabled = false;
            }
        }

        private async void btnAddDelivery_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DeliveryDialog(_context);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _context.Deliveries.Add(dialog.Delivery);

                    // Save delivery items if any
                    if (dialog.Delivery.DeliveryItems != null && dialog.Delivery.DeliveryItems.Any())
                    {
                        foreach (var item in dialog.Delivery.DeliveryItems)
                        {
                            item.DeliveryId = dialog.Delivery.DeliveryId;
                            _context.DeliveryItems.Add(item);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await LoadDeliveriesAsync();
                    MessageBox.Show("Levering succesvol toegevoegd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void btnEditDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (dgDeliveries.SelectedItem is Delivery selectedDelivery)
            {
                var dialog = new DeliveryDialog(_context, selectedDelivery);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        _context.Deliveries.Update(selectedDelivery);

                        // Update delivery items if modified
                        if (dialog.Delivery.DeliveryItems != null)
                        {
                            // Remove old items that are not in the new list
                            var existingItems = await _context.DeliveryItems
                                .Where(di => di.DeliveryId == selectedDelivery.DeliveryId)
                                .ToListAsync();

                            foreach (var existingItem in existingItems)
                            {
                                if (!dialog.Delivery.DeliveryItems.Any(i => i.DeliveryItemId == existingItem.DeliveryItemId))
                                {
                                    _context.DeliveryItems.Remove(existingItem);
                                }
                            }

                            // Add or update items
                            foreach (var item in dialog.Delivery.DeliveryItems)
                            {
                                if (item.DeliveryItemId == 0)
                                {
                                    item.DeliveryId = selectedDelivery.DeliveryId;
                                    _context.DeliveryItems.Add(item);
                                }
                                else
                                {
                                    _context.DeliveryItems.Update(item);
                                }
                            }
                        }

                        await _context.SaveChangesAsync();
                        await LoadDeliveriesAsync();
                        await LoadProductsAsync(); // Reload products als voorraad is gewijzigd
                        await LoadStockAdjustmentsAsync(); // Reload adjustments
                        await LoadLowStockProductsAsync(); // Reload alerts
                        MessageBox.Show("Levering succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void btnProcessDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (dgDeliveries.SelectedItem is Delivery selectedDelivery)
            {
                if (selectedDelivery.IsProcessed)
                {
                    MessageBox.Show("Deze levering is al verwerkt!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Check of levering geannuleerd is
                if (selectedDelivery.Status == DeliveryConstants.Status.Cancelled)
                {
                    MessageBox.Show("Geannuleerde leveringen kunnen niet verwerkt worden!", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string actionText = selectedDelivery.DeliveryType == DeliveryConstants.Types.Incoming 
                    ? "voorraad verhogen" 
                    : "voorraad verlagen";

                var result = MessageBox.Show(
                    $"Weet u zeker dat u levering '{selectedDelivery.ReferenceNumber}' wilt verwerken?\n\n" +
                    $"Type: {selectedDelivery.DeliveryType}\n" +
                    $"Dit zal de {actionText}.",
                    "Levering Verwerken",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // FIXED: Haal alle delivery items op MET Product relatie
                        var deliveryItems = await _context.DeliveryItems
                            .Include(di => di.Product)
                            .Where(di => di.DeliveryId == selectedDelivery.DeliveryId && !di.IsDeleted)
                            .ToListAsync();

                        if (!deliveryItems.Any())
                        {
                            MessageBox.Show("Deze levering heeft geen items!", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        bool isIncoming = selectedDelivery.DeliveryType == DeliveryConstants.Types.Incoming;

                        // VALIDATIE VOOR OUTGOING LEVERINGEN
                        if (!isIncoming)
                        {
                            var validationErrors = new System.Text.StringBuilder();
                            
                            foreach (var item in deliveryItems)
                            {
                                if (item.Product == null) continue;

                                if (item.Product.StockQuantity < item.Quantity)
                                {
                                    validationErrors.AppendLine(
                                        $"- {item.Product.ProductName}: Beschikbaar {item.Product.StockQuantity}, Nodig {item.Quantity}");
                                }
                            }

                            if (validationErrors.Length > 0)
                            {
                                MessageBox.Show(
                                    "ONVOLDOENDE VOORRAAD!\n\n" +
                                    "De volgende producten hebben onvoldoende voorraad:\n\n" +
                                    validationErrors.ToString() +
                                    "\nDe levering kan niet verwerkt worden.",
                                    "Voorraad Fout",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                return;
                            }
                        }

                        // VERWERK DE LEVERING
                        int itemsProcessed = 0;
                        var processingDetails = new System.Text.StringBuilder();
                        processingDetails.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                        processingDetails.AppendLine($"LEVERING VERWERKT: {selectedDelivery.ReferenceNumber}");
                        processingDetails.AppendLine($"Type: {selectedDelivery.DeliveryType}");
                        processingDetails.AppendLine($"Datum: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
                        processingDetails.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                        processingDetails.AppendLine();

                        foreach (var item in deliveryItems)
                        {
                            if (item.Product == null) continue;

                            int previousQty = item.Product.StockQuantity;
                            int quantityChange = isIncoming ? item.Quantity : -item.Quantity;
                            int newQty = previousQty + quantityChange;

                            item.Product.StockQuantity = newQty;

                            // Voeg details toe aan de samenvatting
                            string changeSymbol = isIncoming ? "+" : "-";
                            processingDetails.AppendLine($"PRODUCT: {item.Product.ProductName}");
                            processingDetails.AppendLine($"   Voorraad: {previousQty} {changeSymbol} {Math.Abs(quantityChange)} = {newQty}");
                            processingDetails.AppendLine($"   Prijs: EUR {item.UnitPrice:F2} x {item.Quantity} = EUR {item.Total:F2}");
                            
                            // Check voorraad status
                            if (newQty < item.Product.MinimumStock)
                            {
                                processingDetails.AppendLine($"   WAARSCHUWING: Onder minimum ({item.Product.MinimumStock})");
                            }
                            else if (previousQty < item.Product.MinimumStock && newQty >= item.Product.MinimumStock)
                            {
                                processingDetails.AppendLine($"   HERSTELD: Weer boven minimum voorraad!");
                            }
                            processingDetails.AppendLine();

                            var adjustment = new StockAdjustment
                            {
                                ProductId = item.ProductId,
                                AdjustmentType = isIncoming ? StockAdjustmentConstants.Types.Addition : StockAdjustmentConstants.Types.Removal,
                                QuantityChange = quantityChange,
                                PreviousQuantity = previousQty,
                                NewQuantity = newQty,
                                Reason = $"{selectedDelivery.DeliveryType} levering {selectedDelivery.ReferenceNumber} verwerkt",
                                AdjustedBy = _currentUser.FullName,
                                AdjustmentDate = DateTime.Now
                            };

                            _context.StockAdjustments.Add(adjustment);
                            item.IsProcessed = true;
                            itemsProcessed++;
                        }

                        // Update delivery status
                        selectedDelivery.IsProcessed = true;
                        selectedDelivery.Status = DeliveryConstants.Status.Delivered;
                        selectedDelivery.ActualDeliveryDate = DateTime.Now;

                        await _context.SaveChangesAsync();
                        
                        await LoadDeliveriesAsync();
                        await LoadProductsAsync();
                        await LoadStockAdjustmentsAsync();
                        await LoadLowStockProductsAsync();
                        
                        processingDetails.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                        processingDetails.AppendLine($"TOTAAL: {itemsProcessed} product(en) verwerkt");
                        processingDetails.AppendLine($"Totaalbedrag: EUR {selectedDelivery.TotalAmount:F2}");
                        processingDetails.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                        MessageBox.Show(
                            processingDetails.ToString(),
                            "Levering Succesvol Verwerkt",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij verwerken: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void btnDeleteDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (dgDeliveries.SelectedItem is Delivery selectedDelivery)
            {
                var result = MessageBox.Show(
                    $"Weet u zeker dat u levering '{selectedDelivery.ReferenceNumber}' wilt verwijderen?",
                    "Levering Verwijderen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // SOFT DELETE
                        selectedDelivery.IsDeleted = true;
                        selectedDelivery.DeletedDate = DateTime.Now;
                        _context.Deliveries.Update(selectedDelivery);
                        await _context.SaveChangesAsync();
                        await LoadDeliveriesAsync();
                        MessageBox.Show("Levering succesvol verwijderd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij het verwijderen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void dgVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnEditVehicle.IsEnabled = dgVehicles.SelectedItem != null;
            btnDeleteVehicle.IsEnabled = dgVehicles.SelectedItem != null;
        }

        private async void btnAddVehicle_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VehicleDialog();
            if (dialog.ShowDialog() == true)
            {
                _context.Vehicles.Add(dialog.Vehicle);
                await _context.SaveChangesAsync();
                await LoadVehiclesAsync();
                MessageBox.Show("Voertuig succesvolgevoegd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void btnEditVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (dgVehicles.SelectedItem is Vehicle selectedVehicle)
            {
                var dialog = new VehicleDialog(selectedVehicle);
                if (dialog.ShowDialog() == true)
                {
                    _context.Vehicles.Update(selectedVehicle);
                    await _context.SaveChangesAsync();
                    await LoadVehiclesAsync();
                    MessageBox.Show("Voertuig succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void btnDeleteVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (dgVehicles.SelectedItem is Vehicle selectedVehicle)
            {
                var result = MessageBox.Show(
                    $"Weet u zeker dat u voertuig '{selectedVehicle.LicensePlate}' wilt verwijderen?",
                    "Voertuig Verwijderen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // SOFT DELETE
                        selectedVehicle.IsDeleted = true;
                        selectedVehicle.DeletedDate = DateTime.Now;
                        _context.Vehicles.Update(selectedVehicle);
                        await _context.SaveChangesAsync();
                        await LoadVehiclesAsync();
                        MessageBox.Show("Voertuig succesvol verwijderd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Fout bij het verwijderen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // =====================================================================
        // USER MANAGEMENT METHODS (Administrator only)
        // =====================================================================

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = dgUsers.SelectedItem != null;
            btnManageRoles.IsEnabled = hasSelection;
            btnToggleActive.IsEnabled = hasSelection;
            btnResetPassword.IsEnabled = hasSelection;
        }

        private async void btnManageRoles_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is ApplicationUser selectedUser)
            {
                var dialog = new UserRolesDialog(_context, selectedUser);
                if (dialog.ShowDialog() == true)
                {
                    await LoadUsersAsync();
                    MessageBox.Show($"Rollen voor {selectedUser.FullName} zijn bijgewerkt!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void btnToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is ApplicationUser selectedUser)
            {
                // Voorkom dat admin zichzelf deactiveren
                if (selectedUser.Id == _currentUser.Id)
                {
                    MessageBox.Show("U kunt uw eigen account niet deactiveren!", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string action = selectedUser.IsActive ? "deactiveren" : "activeren";
                var result = MessageBox.Show(
                    $"Weet u zeker dat u het account van '{selectedUser.FullName}' wilt {action}?",
                    $"Account {action}",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        selectedUser.IsActive = !selectedUser.IsActive;
                        _context.Users.Update(selectedUser);
                        await _context.SaveChangesAsync();
                        await LoadUsersAsync();
                        MessageBox.Show($"Account succesvol ge{action}d!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij {action}: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is ApplicationUser selectedUser)
            {
                var result = MessageBox.Show(
                    $"Weet u zeker dat u het wachtwoord wilt resetten voor '{selectedUser.FullName}'?\n\n" +
                    $"Het nieuwe wachtwoord wordt: Reset@123",
                    "Wachtwoord Resetten",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var passwordHasher = new PasswordHasher<ApplicationUser>();
                        selectedUser.PasswordHash = passwordHasher.HashPassword(selectedUser, "Reset@123");
                        _context.Users.Update(selectedUser);
                        await _context.SaveChangesAsync();

                        MessageBox.Show(
                            $"Wachtwoord succesvol gereset!\n\n" +
                            $"Nieuw wachtwoord: Reset@123\n\n" +
                            $"De gebruiker wordt gevraagd dit te wijzigen bij volgende login.",
                            "Succes",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij wachtwoord reset: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnGenerateReports_Click(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new ReportsWindow();
            reportsWindow.Owner = this;
            reportsWindow.ShowDialog();
        }
    }
}