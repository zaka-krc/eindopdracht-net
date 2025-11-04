using SuntoryManagementSystem.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            dgSuppliers.ItemsSource = _context.Suppliers.OrderBy(s => s.SupplierName).ToList();
        }

        private void LoadProducts()
        {
            dgProducts.ItemsSource = _context.Products.OrderBy(p => p.ProductName).ToList();
        }

        private void LoadDeliveries()
        {
            dgDeliveries.ItemsSource = _context.Deliveries.OrderByDescending(d => d.ExpectedDeliveryDate).ToList();
        }

        private void LoadVehicles()
        {
            dgVehicles.ItemsSource = _context.Vehicles.OrderBy(v => v.LicensePlate).ToList();
        }

        private void LoadStockAlerts()
        {
            dgStockAlerts.ItemsSource = _context.StockAlerts.Where(sa => sa.Status == "Active").OrderBy(sa => sa.CurrentStock).ToList();
        }

        private void LoadStockAdjustments()
        {
            dgStockAdjustments.ItemsSource = _context.StockAdjustments.OrderByDescending(sa => sa.AdjustmentDate).ToList();
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
                        _context.Suppliers.Remove(selectedSupplier);
                        _context.SaveChanges();
                        LoadSuppliers();
                        MessageBox.Show("Leverancier succesvol verwijderd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Fout bij het verwijderen: {ex.Message}\n\nMogelijk zijn er nog producten of leveringen gekoppeld aan deze leverancier.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        _context.Products.Remove(selectedProduct);
                        _context.SaveChanges();
                        LoadProducts();
                        MessageBox.Show("Product succesvol verwijderd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Fout bij het verwijderen: {ex.Message}\n\nMogelijk zijn er nog leveringen of aanpassingen gekoppeld aan dit product.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void dgDeliveries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnEditDelivery.IsEnabled = dgDeliveries.SelectedItem != null;
            btnDeleteDelivery.IsEnabled = dgDeliveries.SelectedItem != null;
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
                    MessageBox.Show("Levering succesvol gewijzigd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
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
                        _context.Deliveries.Remove(selectedDelivery);
                        _context.SaveChanges();
                        LoadDeliveries();
                        MessageBox.Show("Levering succesvol verwijderd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
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
                        _context.Vehicles.Remove(selectedVehicle);
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