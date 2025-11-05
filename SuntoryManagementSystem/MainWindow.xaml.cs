using SuntoryManagementSystem.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace SuntoryManagementSystem
{
    public partial class MainWindow : Window
    {
        private readonly SuntoryDbContext _context;

        public MainWindow()
        {
            InitializeComponent();
            _context = new SuntoryDbContext();
            SuntoryDbContext.Seeder(_context);
            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadSuppliers();
            LoadProducts();
            LoadDeliveries();
            LoadVehicles();
            LoadStockAlerts();
            LoadStockAdjustments();
        }

        private void LoadSuppliers()
        {
            dgSuppliers.ItemsSource = _context.Suppliers
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.SupplierName)
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

        private void LoadStockAlerts()
        {
            dgStockAlerts.ItemsSource = _context.StockAlerts
                .Include(sa => sa.Product)
                .Where(sa => !sa.IsDeleted && sa.Status == "Active")
                .OrderBy(sa => sa.Product.StockQuantity)
                .ToList();
        }

        private void LoadStockAdjustments()
        {
            dgStockAdjustments.ItemsSource = _context.StockAdjustments
                .Where(sa => !sa.IsDeleted)
                .OrderByDescending(sa => sa.AdjustmentDate)
                .ToList();
        }

        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcMain.SelectedIndex == 4)
                LoadStockAlerts();
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
                LoadProducts();
                MessageBox.Show("Product succesvol toegevoegd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product selectedProduct)
            {
                var dialog = new ProductDialog(_context, selectedProduct);
                if (dialog.ShowDialog() == true)
                {
                    _context.Products.Update(selectedProduct);
                    _context.SaveChanges();
                    LoadProducts();
                    MessageBox.Show("Product succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
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
                        // SOFT DELETE
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

        private void dgDeliveries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = dgDeliveries.SelectedItem != null;
            bool isNotProcessed = hasSelection && dgDeliveries.SelectedItem is Delivery delivery && !delivery.IsProcessed;
            
            btnEditDelivery.IsEnabled = hasSelection;
            btnDeleteDelivery.IsEnabled = hasSelection;
            btnProcessDelivery.IsEnabled = isNotProcessed; // Alleen tonen als niet verwerkt
        }

        private void btnAddDelivery_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DeliveryDialog(_context);
            if (dialog.ShowDialog() == true)
            {
                _context.Deliveries.Add(dialog.Delivery);
                _context.SaveChanges();
                LoadDeliveries();
                MessageBox.Show("Levering succesvol toegevoegd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnEditDelivery_Click(object sender, RoutedEventArgs e)
        {
            if (dgDeliveries.SelectedItem is Delivery selectedDelivery)
            {
                var dialog = new DeliveryDialog(_context, selectedDelivery);
                if (dialog.ShowDialog() == true)
                {
                    _context.Deliveries.Update(selectedDelivery);
                    _context.SaveChanges();
                    LoadDeliveries();
                    LoadProducts(); // Reload products als voorraad is gewijzigd
                    LoadStockAdjustments(); // Reload adjustments
                    LoadStockAlerts(); // Reload alerts
                    MessageBox.Show("Levering succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
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

                var result = MessageBox.Show(
                    $"Weet u zeker dat u levering '{selectedDelivery.ReferenceNumber}' wilt verwerken?\n\n" +
                    "Dit zal de voorraad automatisch bijwerken.",
                    "Levering Verwerken",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Markeer als verwerkt en verwerk via dialog logic
                        selectedDelivery.IsProcessed = true;
                        
                        // Haal alle delivery items op
                        var deliveryItems = _context.DeliveryItems
                            .Include(di => di.Product)
                            .Where(di => di.DeliveryId == selectedDelivery.DeliveryId && !di.IsDeleted)
                            .ToList();

                        if (!deliveryItems.Any())
                        {
                            MessageBox.Show("Deze levering heeft geen items!", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                            selectedDelivery.IsProcessed = false;
                            return;
                        }

                        int itemsProcessed = 0;
                        foreach (var item in deliveryItems)
                        {
                            if (item.Product == null) continue;

                            int previousQty = item.Product.StockQuantity;
                            int newQty = previousQty + item.Quantity;
                            item.Product.StockQuantity = newQty;

                            var adjustment = new StockAdjustment
                            {
                                ProductId = item.ProductId,
                                AdjustmentType = "Addition",
                                QuantityChange = item.Quantity,
                                PreviousQuantity = previousQty,
                                NewQuantity = newQty,
                                Reason = $"Levering {selectedDelivery.ReferenceNumber} verwerkt",
                                AdjustedBy = "Systeem",
                                AdjustmentDate = DateTime.Now
                            };

                            _context.StockAdjustments.Add(adjustment);
                            item.IsProcessed = true;
                            itemsProcessed++;

                            // Resolve alerts indien voorraad boven minimum
                            if (newQty >= item.Product.MinimumStock)
                            {
                                var alerts = _context.StockAlerts
                                    .Where(sa => sa.ProductId == item.ProductId && sa.Status == "Active" && !sa.IsDeleted)
                                    .ToList();
                                
                                foreach (var alert in alerts)
                                {
                                    alert.Status = "Resolved";
                                    alert.ResolvedDate = DateTime.Now;
                                }
                            }
                        }

                        if (selectedDelivery.Status != "Delivered")
                            selectedDelivery.Status = "Delivered";
                        
                        if (selectedDelivery.ActualDeliveryDate == null)
                            selectedDelivery.ActualDeliveryDate = DateTime.Now;

                        _context.SaveChanges();
                        
                        LoadDeliveries();
                        LoadProducts();
                        LoadStockAdjustments();
                        LoadStockAlerts();
                        
                        MessageBox.Show(
                            $"Levering succesvol verwerkt!\n\n{itemsProcessed} product(en) toegevoegd aan voorraad.",
                            "Succes",
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
    }
}