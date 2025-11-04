using SuntoryManagementSystem.Models;
using System.Linq;
using System.Windows;

namespace SuntoryManagementSystem
{
    public partial class ProductDialog : Window
    {
        public Product Product { get; private set; }
        private readonly bool _isEditMode;
        private readonly SuntoryDbContext _context;

        public ProductDialog(SuntoryDbContext context)
        {
            InitializeComponent();
            Title = "Nieuw Product Toevoegen";
            _isEditMode = false;
            _context = context;
            Product = new Product();
            LoadSuppliers();
        }

        public ProductDialog(SuntoryDbContext context, Product product) : this(context)
        {
            Title = "Product Wijzigen";
            _isEditMode = true;
            Product = product;
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
            Product.StockQuantity = stockQuantity;
            Product.MinimumStock = minimumStock;
            Product.IsActive = chkIsActive.IsChecked ?? true;
            Product.Description = txtDescription.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}