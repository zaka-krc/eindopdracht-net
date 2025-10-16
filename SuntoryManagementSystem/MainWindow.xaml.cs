using SuntoryManagementSystem.Models;
using System.Linq;
using System.Windows;

namespace SuntoryManagementSystem
{
    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        private readonly SuntoryDbContext _context;

        public MainWindow()
        {
            InitializeComponent();

            // Initialiseer de database context
            _context = new SuntoryDbContext();

            // Seed de database met initiële data
            SuntoryDbContext.Seeder(_context);

            // Laad alle data bij het opstarten
            LoadAllData();
        }

        /// Laadt alle data van de database in de DataGrids
        private void LoadAllData()
        {
            LoadSuppliers();
            LoadProducts();
            LoadDeliveries();
            LoadVehicles();
            LoadStockAlerts();
            LoadStockAdjustments();
        }

        /// Laadt leveranciers in de DataGrid
        private void LoadSuppliers()
        {
            dgSuppliers.ItemsSource = _context.Suppliers
                .OrderBy(s => s.SupplierName)
                .ToList();
        }

        /// Laadt producten in de DataGrid
        private void LoadProducts()
        {
            dgProducts.ItemsSource = _context.Products
                .OrderBy(p => p.ProductName)
                .ToList();
        }

        /// Laadt leveringen in de DataGrid
        private void LoadDeliveries()
        {
            dgDeliveries.ItemsSource = _context.Deliveries
                .OrderByDescending(d => d.ExpectedDeliveryDate)
                .ToList();
        }

        /// Laadt voertuigen in de DataGrid
        private void LoadVehicles()
        {
            dgVehicles.ItemsSource = _context.Vehicles
                .OrderBy(v => v.LicensePlate)
                .ToList();
        }

        /// Laadt voorraad alerts in de DataGrid
        private void LoadStockAlerts()
        {
            dgStockAlerts.ItemsSource = _context.StockAlerts
                .Where(sa => sa.Status == "Active")
                .OrderBy(sa => sa.CurrentStock)
                .ToList();
        }

        /// Laadt voorraad aanpassingen in de DataGrid
        private void LoadStockAdjustments()
        {
            dgStockAdjustments.ItemsSource = _context.StockAdjustments
                .OrderByDescending(sa => sa.AdjustmentDate)
                .ToList();
        }

        /// Sluit het venster wanneer database context disposed moet worden
        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}