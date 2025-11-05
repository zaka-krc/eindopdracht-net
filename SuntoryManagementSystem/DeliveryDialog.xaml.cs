using SuntoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace SuntoryManagementSystem
{
    public partial class DeliveryDialog : Window
    {
        public Delivery Delivery { get; private set; }
        private readonly bool _isEditMode;
        private readonly SuntoryDbContext _context;
        private bool _wasProcessed;
        private ObservableCollection<DeliveryItem> _items;

        public DeliveryDialog(SuntoryDbContext context)
        {
            InitializeComponent();
            Title = "Nieuwe Levering Toevoegen";
            _isEditMode = false;
            _context = context;
            Delivery = new Delivery();
            _wasProcessed = false;
            _items = new ObservableCollection<DeliveryItem>();
            
            LoadComboBoxes();
            LoadProducts();
            dpExpectedDate.SelectedDate = DateTime.Now.AddDays(7);
            dgItems.ItemsSource = _items;
            UpdateTotal();
        }

        public DeliveryDialog(SuntoryDbContext context, Delivery delivery) : this(context)
        {
            Title = "Levering Wijzigen";
            _isEditMode = true;
            Delivery = delivery;
            _wasProcessed = delivery.IsProcessed;
            LoadDeliveryData();
            LoadDeliveryItems();
        }

        private void LoadComboBoxes()
        {
            var suppliers = _context.Suppliers.Where(s => !s.IsDeleted).OrderBy(s => s.SupplierName).ToList();
            cmbSupplier.ItemsSource = suppliers;
            
            var customers = _context.Customers.Where(c => !c.IsDeleted).OrderBy(c => c.CustomerName).ToList();
            cmbCustomer.ItemsSource = customers;
            
            var vehicles = _context.Vehicles.Where(v => !v.IsDeleted).OrderBy(v => v.LicensePlate).ToList();
            cmbVehicle.ItemsSource = vehicles;
            
            if (suppliers.Any())
                cmbSupplier.SelectedIndex = 0;
            
            // Set initial visibility to Incoming
            UpdateVisibility("Incoming");
        }

        private void LoadProducts()
        {
            var products = _context.Products
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.ProductName)
                .ToList();
            
            cmbProduct.ItemsSource = products;
            if (products.Any())
            {
                cmbProduct.SelectedIndex = 0;
            }
        }

        private void LoadDeliveryItems()
        {
            if (_isEditMode)
            {
                var items = _context.DeliveryItems
                    .Include(di => di.Product)
                    .Where(di => di.DeliveryId == Delivery.DeliveryId && !di.IsDeleted)
                    .ToList();
                
                _items.Clear();
                foreach (var item in items)
                {
                    _items.Add(item);
                }
                UpdateTotal();
            }
        }

        private void LoadDeliveryData()
        {
            // Set delivery type
            if (!string.IsNullOrEmpty(Delivery.DeliveryType))
            {
                cmbDeliveryType.SelectedIndex = Delivery.DeliveryType == "Incoming" ? 0 : 1;
            }
            
            txtReferenceNumber.Text = Delivery.ReferenceNumber;
            cmbSupplier.SelectedValue = Delivery.SupplierId;
            cmbCustomer.SelectedValue = Delivery.CustomerId;
            cmbVehicle.SelectedValue = Delivery.VehicleId;
            dpExpectedDate.SelectedDate = Delivery.ExpectedDeliveryDate;
            
            // Set status via loop through items
            foreach (ComboBoxItem item in cmbStatus.Items)
            {
                if (item.Content.ToString() == Delivery.Status)
                {
                    cmbStatus.SelectedItem = item;
                    break;
                }
            }
            
            txtNotes.Text = Delivery.Notes;
            
            UpdateVisibility(Delivery.DeliveryType);
        }

        private void cmbDeliveryType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDeliveryType.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                string type = item.Tag.ToString()!;
                UpdateVisibility(type);
            }
        }

        private void UpdateVisibility(string deliveryType)
        {
            if (string.IsNullOrEmpty(deliveryType))
                deliveryType = "Incoming";
                
            bool isIncoming = deliveryType == "Incoming";
            
            // Check of controls bestaan (voor initialisatie problemen)
            if (lblSupplier != null && cmbSupplier != null && lblCustomer != null && cmbCustomer != null)
            {
                lblSupplier.Visibility = isIncoming ? Visibility.Visible : Visibility.Collapsed;
                cmbSupplier.Visibility = isIncoming ? Visibility.Visible : Visibility.Collapsed;
                
                lblCustomer.Visibility = isIncoming ? Visibility.Collapsed : Visibility.Visible;
                cmbCustomer.Visibility = isIncoming ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void cmbProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProduct.SelectedItem is Product product)
            {
                string deliveryType = ((ComboBoxItem)cmbDeliveryType.SelectedItem).Tag.ToString()!;
                decimal price = deliveryType == "Incoming" 
                    ? product.PurchasePrice 
                    : product.SellingPrice;
                
                txtUnitPrice.Text = price.ToString("F2");
            }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProduct.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een product!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Voer een geldige hoeveelheid in!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                return;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice < 0)
            {
                MessageBox.Show("Voer een geldige prijs in!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUnitPrice.Focus();
                return;
            }

            var product = (Product)cmbProduct.SelectedItem;
            string deliveryType = ((ComboBoxItem)cmbDeliveryType.SelectedItem).Tag.ToString()!;

            // Check bij outgoing of er genoeg voorraad is
            if (deliveryType == "Outgoing" && product.StockQuantity < quantity)
            {
                var result = MessageBox.Show(
                    $"Let op: Je probeert {quantity} stuks toe te voegen, maar er zijn slechts {product.StockQuantity} in voorraad.\n\n" +
                    "Wil je toch doorgaan?",
                    "Voorraad Waarschuwing",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                    return;
            }

            var item = new DeliveryItem
            {
                ProductId = product.ProductId,
                Product = product,
                Quantity = quantity,
                UnitPrice = unitPrice,
                IsProcessed = false
            };

            _items.Add(item);
            UpdateTotal();

            // Reset form
            txtQuantity.Text = "1";
            if (cmbProduct.SelectedItem is Product selectedProduct)
            {
                decimal price = deliveryType == "Incoming" 
                    ? selectedProduct.PurchasePrice 
                    : selectedProduct.SellingPrice;
                txtUnitPrice.Text = price.ToString("F2");
            }
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (dgItems.SelectedItem is DeliveryItem selectedItem)
            {
                var result = MessageBox.Show(
                    "Weet u zeker dat u dit item wilt verwijderen?",
                    "Item Verwijderen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _items.Remove(selectedItem);
                    UpdateTotal();
                }
            }
        }

        private void dgItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnRemoveItem.IsEnabled = dgItems.SelectedItem != null;
        }

        private void UpdateTotal()
        {
            decimal total = _items.Sum(i => i.Quantity * i.UnitPrice);
            txtTotal.Text = string.Format("Totaal: €{0:N2}", total);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReferenceNumber.Text))
            {
                MessageBox.Show("Referentienummer is verplicht!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtReferenceNumber.Focus();
                return;
            }

            string deliveryType = ((ComboBoxItem)cmbDeliveryType.SelectedItem).Tag.ToString()!;
            
            if (deliveryType == "Incoming" && cmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een leverancier!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbSupplier.Focus();
                return;
            }

            if (deliveryType == "Outgoing" && cmbCustomer.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een klant!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbCustomer.Focus();
                return;
            }

            if (dpExpectedDate.SelectedDate == null)
            {
                MessageBox.Show("Selecteer een verwachte datum!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpExpectedDate.Focus();
                return;
            }

            if (!_items.Any())
            {
                MessageBox.Show("Voeg minimaal een product toe aan de levering!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Delivery.DeliveryType = deliveryType;
            Delivery.ReferenceNumber = txtReferenceNumber.Text.Trim();
            
            if (deliveryType == "Incoming")
            {
                Delivery.SupplierId = (int)cmbSupplier.SelectedValue;
                Delivery.CustomerId = null;
            }
            else
            {
                Delivery.CustomerId = (int)cmbCustomer.SelectedValue;
                Delivery.SupplierId = null;
            }
            
            Delivery.VehicleId = cmbVehicle.SelectedValue as int?;
            Delivery.ExpectedDeliveryDate = dpExpectedDate.SelectedDate.Value;
            Delivery.Status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString()!;
            Delivery.TotalAmount = _items.Sum(i => i.Quantity * i.UnitPrice);
            Delivery.Notes = txtNotes.Text.Trim();

            if (Delivery.Status == "Delivered" && Delivery.ActualDeliveryDate == null)
            {
                Delivery.ActualDeliveryDate = DateTime.Now;
            }

            if (!_isEditMode)
            {
                Delivery.CreatedDate = DateTime.Now;
            }

            // Store items with the delivery
            Delivery.DeliveryItems = _items.ToList();

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