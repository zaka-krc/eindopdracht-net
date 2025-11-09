using SuntoryManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SuntoryManagementSystem
{
    public partial class MainWindow : Window
    {
        private readonly SuntoryDbContext _context;
        private readonly ApplicationUser _currentUser;
        private readonly List<string> _userRoles;
        private bool _isSwitchingWindows = false; // Nieuwe vlag om window switch te detecteren

        // Constructor ZONDER parameters - Start in GUEST MODE
        public MainWindow() : this(null, new List<string>())
        {
        }

        // Nieuwe constructor MET ingelogde gebruiker en rollen
        public MainWindow(ApplicationUser? currentUser, List<string> userRoles)
        {
            InitializeComponent();
            
            _context = new SuntoryDbContext();
            _currentUser = currentUser ?? new ApplicationUser 
            { 
                FullName = "Gast (Alleen-lezen)", 
                Email = "guest@suntory.com",
                Id = "guest"
            };
            _userRoles = userRoles ?? new List<string> { "Guest" };

            // Set user info in header
            txtUserName.Text = _currentUser.FullName;
            txtUserRole.Text = string.Join(", ", _userRoles.Count == 1 && _userRoles[0] == "Guest" ? new[] { "Gast - Alleen Lezen" } : _userRoles);

            // Configure buttons based on login status
            bool isGuest = _userRoles.Contains("Guest") || _userRoles.Count == 0 || currentUser == null;
            btnLogin.Visibility = isGuest ? Visibility.Visible : Visibility.Collapsed;
            btnRegister.Visibility = isGuest ? Visibility.Visible : Visibility.Collapsed;
            btnLogout.Visibility = isGuest ? Visibility.Collapsed : Visibility.Visible;

            // Configure menu based on roles
            ConfigureMenuForRoles();

            SuntoryDbContext.Seeder(_context);
            LoadAllData();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Open register window
            var registerWindow = new RegisterWindow();
            if (registerWindow.ShowDialog() == true && registerWindow.NewUser != null)
            {
                MessageBox.Show(
                    $"Welkom, {registerWindow.NewUser.FullName}!\n\n" +
                    "Uw account is aangemaakt.\n" +
                    "U heeft alleen-lezen rechten.\n\n" +
                    "Neem contact op met een administrator voor extra rechten.",
                    "Registratie Succesvol",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // FIXED: Open nieuw window EERST, dan sluit oude window
                _isSwitchingWindows = true;
                var mainWindow = new MainWindow(registerWindow.NewUser, new List<string> { "Guest" });
                mainWindow.Show();
                this.Close();
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Open login window
            var loginWindow = new LoginWindow();
            
            if (loginWindow.ShowDialog() == true && loginWindow.LoggedInUser != null)
            {
                // Haal de rollen op van de ingelogde gebruiker
                var userRoles = (from ur in _context.UserRoles
                                where ur.UserId == loginWindow.LoggedInUser.Id
                                join r in _context.Roles on ur.RoleId equals r.Id
                                select r.Name).ToList();

                // FIXED: Open nieuw window EERST, dan sluit oude window
                _isSwitchingWindows = true;
                var mainWindow = new MainWindow(loginWindow.LoggedInUser, userRoles);
                mainWindow.Show();
                this.Close();
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
            bool isGuest = _userRoles.Contains("Guest") || _userRoles.Count == 0;

            // GUEST MODE: Alleen-lezen toegang (alles zichtbaar, niets aanpasbaar)
            if (isGuest)
            {
                // Verberg alle wijzigingsknoppen
                tabUserManagement.Visibility = Visibility.Collapsed;
                
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
                
                return;
            }

            // ADMINISTRATOR: Volledige toegang + User Management
            if (isAdmin)
            {
                tabUserManagement.Visibility = Visibility.Visible;
                // Admin heeft toegang tot alles
                return;
            }

            // MANAGER: Geen User Management, wel alles anders
            if (isManager)
            {
                tabUserManagement.Visibility = Visibility.Collapsed;
                // Manager heeft toegang tot alle operationele tabs
                return;
            }

            // EMPLOYEE: Beperkte toegang (geen verwijderen, geen voertuigen)
            if (isEmployee)
            {
                tabUserManagement.Visibility = Visibility.Collapsed;
                tabVehicles.Visibility = Visibility.Collapsed;

                // Verberg delete buttons voor employees
                btnDeleteSupplier.Visibility = Visibility.Collapsed;
                btnDeleteCustomer.Visibility = Visibility.Collapsed;
                btnDeleteProduct.Visibility = Visibility.Collapsed;
                btnDeleteDelivery.Visibility = Visibility.Collapsed;
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Weet u zeker dat u wilt uitloggen, {_currentUser.FullName}?\n\n" +
                "U keert terug naar de alleen-lezen modus.",
                "Uitloggen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // FIXED: Open nieuw window EERST, dan sluit oude window
                _isSwitchingWindows = true;
                var guestWindow = new MainWindow();
                guestWindow.Show();
                this.Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Sluit alleen de applicatie als we niet bezig zijn met een window switch
            if (!_isSwitchingWindows)
            {
                Application.Current.Shutdown();
            }
        }

        private void LoadAllData()
        {
            try
            {
                LoadSuppliers();
                LoadCustomers();
                LoadProducts();
                LoadDeliveries();
                LoadVehicles();
                LoadLowStockProducts();
                LoadStockAdjustments();
                LoadUsers(); // NEW: Load users for admin panel
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden van data: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // NEW: Load users voor User Management tab
        private void LoadUsers()
        {
            try
            {
                // LINQ Query Syntax om alle gebruikers te laden
                var users = (from u in _context.Users
                            orderby u.IsActive descending, u.FullName
                            select u).ToList();

                dgUsers.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden van gebruikers: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSuppliers()
        {
            dgSuppliers.ItemsSource = _context.Suppliers
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Status == "Active" ? 0 : 1) // Actieve eerst
                .ThenBy(s => s.SupplierName)
                .ToList();
        }

        private void LoadCustomers()
        {
            dgCustomers.ItemsSource = _context.Customers
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Status == "Active" ? 0 : 1) // Actieve eerst
                .ThenBy(c => c.CustomerName)
                .ToList();
        }

        private void LoadProducts()
        {
            dgProducts.ItemsSource = _context.Products
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.ProductName)
                .ToList();
        }

        private void LoadDeliveries()
        {
            dgDeliveries.ItemsSource = _context.Deliveries
                .Include(d => d.Supplier)
                .Include(d => d.Customer)
                .Include(d => d.Vehicle)
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.ExpectedDeliveryDate)
                .ToList();
        }

        private void LoadVehicles()
        {
            dgVehicles.ItemsSource = _context.Vehicles
                .Where(v => !v.IsDeleted)
                .OrderBy(v => v.LicensePlate)
                .ToList();
        }

        private void LoadLowStockProducts()
        {
            // Gewoon producten tonen waar voorraad onder minimum is
            dgStockAlerts.ItemsSource = _context.Products
                .Include(p => p.Supplier)
                .Where(p => !p.IsDeleted && p.IsActive)
                .Where(p => p.StockQuantity < p.MinimumStock)
                .OrderBy(p => p.StockQuantity)
                .ToList();
        }

        private void LoadStockAdjustments()
        {
            dgStockAdjustments.ItemsSource = _context.StockAdjustments
                .Include(sa => sa.Product)  // Laad Product relatie
                .Where(sa => !sa.IsDeleted)
                .OrderByDescending(sa => sa.AdjustmentDate)
                .ToList();
        }

        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcMain.SelectedIndex == 4)
                LoadLowStockProducts(); // Refresh wanneer tab wordt geopend
            else if (tcMain.SelectedIndex == 5)
                LoadStockAdjustments();
        }

        private void dgSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnEditSupplier.IsEnabled = dgSuppliers.SelectedItem != null;
            btnDeleteSupplier.IsEnabled = dgSuppliers.SelectedItem != null;
        }

        private void btnAddSupplier_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SupplierDialog();
            if (dialog.ShowDialog() == true)
            {
                _context.Suppliers.Add(dialog.Supplier);
                _context.SaveChanges();
                LoadSuppliers();
                MessageBox.Show("Leverancier succesvol toegevoegd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnEditSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (dgSuppliers.SelectedItem is Supplier selectedSupplier)
            {
                var dialog = new SupplierDialog(selectedSupplier);
                if (dialog.ShowDialog() == true)
                {
                    _context.Suppliers.Update(selectedSupplier);
                    _context.SaveChanges();
                    LoadSuppliers();
                    MessageBox.Show("Leverancier succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void btnDeleteSupplier_Click(object sender, RoutedEventArgs e)
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
                        _context.SaveChanges();
                        LoadSuppliers();
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

        private void btnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CustomerDialog();
            if (dialog.ShowDialog() == true)
            {
                _context.Customers.Add(dialog.Customer);
                _context.SaveChanges();
                LoadCustomers();
                MessageBox.Show("Klant succesvol toegevoegd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnEditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer selectedCustomer)
            {
                var dialog = new CustomerDialog(selectedCustomer);
                if (dialog.ShowDialog() == true)
                {
                    _context.Customers.Update(selectedCustomer);
                    _context.SaveChanges();
                    LoadCustomers();
                    MessageBox.Show("Klant succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void btnDeleteCustomer_Click(object sender, RoutedEventArgs e)
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
                        _context.SaveChanges();
                        LoadCustomers();
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

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductDialog(_context);
            if (dialog.ShowDialog() == true)
            {
                _context.Products.Add(dialog.Product);
                _context.SaveChanges();

                if (dialog.HasStockAdjustment)
                {
                    CreateStockAdjustmentForNewProduct(dialog.Product, dialog.InitialStock);
                    _context.SaveChanges();
                }

                LoadProducts();
                LoadLowStockProducts();
                LoadStockAdjustments();

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

        private void btnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product selectedProduct)
            {
                int originalStock = selectedProduct.StockQuantity;
                
                var dialog = new ProductDialog(_context, selectedProduct);
                if (dialog.ShowDialog() == true)
                {
                    _context.Products.Update(selectedProduct);
                    _context.SaveChanges();

                    if (dialog.HasStockAdjustment)
                    {
                        CreateStockAdjustment(selectedProduct, originalStock, dialog.InitialStock, dialog.AdjustmentType);
                        _context.SaveChanges();
                    }

                    LoadProducts();
                    LoadLowStockProducts();
                    LoadStockAdjustments();

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

        private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
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
                        _context.SaveChanges();
                        LoadProducts();
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

        private void btnAddDelivery_Click(object sender, RoutedEventArgs e)
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

                    _context.SaveChanges();
                    LoadDeliveries();
                    MessageBox.Show("Levering succesvol toegevoegd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEditDelivery_Click(object sender, RoutedEventArgs e)
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
                            var existingItems = _context.DeliveryItems
                                .Where(di => di.DeliveryId == selectedDelivery.DeliveryId)
                                .ToList();

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

                        _context.SaveChanges();
                        LoadDeliveries();
                        LoadProducts(); // Reload products als voorraad is gewijzigd
                        LoadStockAdjustments(); // Reload adjustments
                        LoadLowStockProducts(); // Reload alerts
                        MessageBox.Show("Levering succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnProcessDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (dgDeliveries.SelectedItem is Delivery selectedDelivery)
            {
                if (selectedDelivery.IsProcessed)
                {
                    MessageBox.Show("Deze levering is al verwerkt!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Check of levering geannuleerd is
                if (selectedDelivery.Status == "Geannuleerd")
                {
                    MessageBox.Show("Geannuleerde leveringen kunnen niet verwerkt worden!", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string actionText = selectedDelivery.DeliveryType == "Incoming" 
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
                        var deliveryItems = _context.DeliveryItems
                            .Include(di => di.Product)
                            .Where(di => di.DeliveryId == selectedDelivery.DeliveryId && !di.IsDeleted)
                            .ToList();

                        if (!deliveryItems.Any())
                        {
                            MessageBox.Show("Deze levering heeft geen items!", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        bool isIncoming = selectedDelivery.DeliveryType == "Incoming";

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
                                AdjustmentType = isIncoming ? "Addition" : "Removal",
                                QuantityChange = quantityChange,
                                PreviousQuantity = previousQty,
                                NewQuantity = newQty,
                                Reason = $"{selectedDelivery.DeliveryType} levering {selectedDelivery.ReferenceNumber} verwerkt",
                                AdjustedBy = _currentUser.FullName,  // Gebruik ingelogde gebruiker
                                AdjustmentDate = DateTime.Now
                            };

                            _context.StockAdjustments.Add(adjustment);
                            item.IsProcessed = true;
                            itemsProcessed++;
                        }

                        // Update delivery status
                        selectedDelivery.IsProcessed = true;
                        selectedDelivery.Status = "Delivered";
                        selectedDelivery.ActualDeliveryDate = DateTime.Now;

                        _context.SaveChanges();
                        
                        LoadDeliveries();
                        LoadProducts();
                        LoadStockAdjustments();
                        LoadLowStockProducts();
                        
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

        private void btnDeleteDelivery_Click(object sender, RoutedEventArgs e)
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
                        _context.SaveChanges();
                        LoadDeliveries();
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

        private void btnAddVehicle_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VehicleDialog();
            if (dialog.ShowDialog() == true)
            {
                _context.Vehicles.Add(dialog.Vehicle);
                _context.SaveChanges();
                LoadVehicles();
                MessageBox.Show("Voertuig succesvol toegevoegd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnEditVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (dgVehicles.SelectedItem is Vehicle selectedVehicle)
            {
                var dialog = new VehicleDialog(selectedVehicle);
                if (dialog.ShowDialog() == true)
                {
                    _context.Vehicles.Update(selectedVehicle);
                    _context.SaveChanges();
                    LoadVehicles();
                    MessageBox.Show("Voertuig succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void btnDeleteVehicle_Click(object sender, RoutedEventArgs e)
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
                        _context.SaveChanges();
                        LoadVehicles();
                        MessageBox.Show("Voertuig succesvol verwijderd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Fout bij het verwijderen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
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

        private void btnManageRoles_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is ApplicationUser selectedUser)
            {
                var dialog = new UserRolesDialog(_context, selectedUser);
                if (dialog.ShowDialog() == true)
                {
                    LoadUsers();
                    MessageBox.Show($"Rollen voor {selectedUser.FullName} zijn bijgewerkt!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void btnToggleActive_Click(object sender, RoutedEventArgs e)
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
                        _context.SaveChanges();
                        LoadUsers();
                        MessageBox.Show($"Account succesvol ge{action}d!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij {action}: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
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
                        _context.SaveChanges();

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
    }
}