using SuntoryManagementSystem.Models;
using System;
using System.Linq;
using System.Windows;

namespace SuntoryManagementSystem
{
    public partial class ProductDialog : Window
    {
        public Product Product { get; private set; }
        private readonly bool _isEditMode;
        private readonly SuntoryDbContext _context;
        private int _originalStockQuantity; // Om voorraadwijzigingen te tracken

        public ProductDialog(SuntoryDbContext context)
        {
            InitializeComponent();
            Title = "Nieuw Product Toevoegen";
            _isEditMode = false;
            _context = context;
            Product = new Product();
            _originalStockQuantity = 0;
            LoadSuppliers();
        }

        public ProductDialog(SuntoryDbContext context, Product product) : this(context)
        {
            Title = "Product Wijzigen";
            _isEditMode = true;
            Product = product;
            _originalStockQuantity = product.StockQuantity; // Bewaar originele voorraad
            LoadProductData();
        }

        private void LoadSuppliers()
        {
            var suppliers = _context.Suppliers.OrderBy(s => s.SupplierName).ToList();
            cmbSupplier.ItemsSource = suppliers;
            
            if (suppliers.Any())
            {
                cmbSupplier.SelectedIndex = 0;
            }
        }

        private void LoadProductData()
        {
            txtProductName.Text = Product.ProductName;
            txtSKU.Text = Product.SKU;
            cmbCategory.Text = Product.Category;
            cmbSupplier.SelectedValue = Product.SupplierId;
            txtPurchasePrice.Text = Product.PurchasePrice.ToString("F2");
            txtSellingPrice.Text = Product.SellingPrice.ToString("F2");
            txtStockQuantity.Text = Product.StockQuantity.ToString();
            txtMinimumStock.Text = Product.MinimumStock.ToString();
            chkIsActive.IsChecked = Product.IsActive;
            txtDescription.Text = Product.Description;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Productnaam is verplicht!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtProductName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSKU.Text))
            {
                MessageBox.Show("SKU is verplicht!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSKU.Focus();
                return;
            }

            if (cmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een leverancier!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbSupplier.Focus();
                return;
            }

            if (!decimal.TryParse(txtPurchasePrice.Text, out decimal purchasePrice) || purchasePrice < 0)
            {
                MessageBox.Show("Voer een geldige inkoopprijs in!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPurchasePrice.Focus();
                return;
            }

            if (!decimal.TryParse(txtSellingPrice.Text, out decimal sellingPrice) || sellingPrice < 0)
            {
                MessageBox.Show("Voer een geldige verkoopprijs in!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSellingPrice.Focus();
                return;
            }

            if (!int.TryParse(txtStockQuantity.Text, out int stockQuantity) || stockQuantity < 0)
            {
                MessageBox.Show("Voer een geldige voorraad in!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStockQuantity.Focus();
                return;
            }

            if (!int.TryParse(txtMinimumStock.Text, out int minimumStock) || minimumStock < 0)
            {
                MessageBox.Show("Voer een geldige minimale voorraad in!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMinimumStock.Focus();
                return;
            }

            Product.ProductName = txtProductName.Text.Trim();
            Product.SKU = txtSKU.Text.Trim();
            Product.Category = cmbCategory.Text.Trim();
            Product.SupplierId = (int)cmbSupplier.SelectedValue;
            Product.PurchasePrice = purchasePrice;
            Product.SellingPrice = sellingPrice;
            Product.MinimumStock = minimumStock;
            Product.IsActive = chkIsActive.IsChecked ?? true;
            Product.Description = txtDescription.Text.Trim();

            // NIEUW: Check of voorraad is gewijzigd en maak StockAdjustment
            if (_isEditMode && stockQuantity != _originalStockQuantity)
            {
                CreateStockAdjustment(_originalStockQuantity, stockQuantity);
            }

            Product.StockQuantity = stockQuantity;

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Maakt automatisch een StockAdjustment record aan bij voorraadwijziging
        /// </summary>
        private void CreateStockAdjustment(int previousQuantity, int newQuantity)
        {
            int quantityChange = newQuantity - previousQuantity;
            string adjustmentType;
            string reason;

            if (quantityChange > 0)
            {
                adjustmentType = "Addition";
                reason = $"Handmatige voorraad aanpassing: +{quantityChange} stuks toegevoegd";
            }
            else
            {
                adjustmentType = "Removal";
                reason = $"Handmatige voorraad aanpassing: {quantityChange} stuks verwijderd";
            }

            var adjustment = new StockAdjustment
            {
                ProductId = Product.ProductId,
                AdjustmentType = adjustmentType,
                QuantityChange = quantityChange,
                PreviousQuantity = previousQuantity,
                NewQuantity = newQuantity,
                Reason = reason,
                AdjustedBy = "Systeem - Handmatige wijziging",
                AdjustmentDate = DateTime.Now
            };

            _context.StockAdjustments.Add(adjustment);

            // Check of we een StockAlert moeten maken
            if (newQuantity < Product.MinimumStock)
            {
                CreateOrUpdateStockAlert(newQuantity);
            }
            else
            {
                // Resolve bestaande alerts als voorraad weer boven minimum is
                ResolveStockAlerts();
            }
        }

        /// <summary>
        /// Maakt een StockAlert aan of update bestaande alert
        /// </summary>
        private void CreateOrUpdateStockAlert(int currentStock)
        {
            var existingAlert = _context.StockAlerts
                .FirstOrDefault(sa => sa.ProductId == Product.ProductId 
                    && sa.Status == "Active" 
                    && !sa.IsDeleted);

            if (existingAlert == null)
            {
                // Maak nieuwe alert
                string alertType = currentStock == 0 ? "Out of Stock" : 
                                 currentStock < (Product.MinimumStock / 2) ? "Critical" : 
                                 "Low Stock";

                var alert = new StockAlert
                {
                    ProductId = Product.ProductId,
                    AlertType = alertType,
                    Status = "Active",
                    CreatedDate = DateTime.Now,
                    Notes = $"Voorraad is {currentStock} stuks, minimum is {Product.MinimumStock}"
                };

                _context.StockAlerts.Add(alert);
            }
            else
            {
                // Update bestaande alert type
                existingAlert.AlertType = currentStock == 0 ? "Out of Stock" : 
                                        currentStock < (Product.MinimumStock / 2) ? "Critical" : 
                                        "Low Stock";
                existingAlert.Notes = $"Voorraad is {currentStock} stuks, minimum is {Product.MinimumStock}";
            }
        }

        /// <summary>
        /// Resolved actieve StockAlerts voor dit product
        /// </summary>
        private void ResolveStockAlerts()
        {
            var activeAlerts = _context.StockAlerts
                .Where(sa => sa.ProductId == Product.ProductId 
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}