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
            
            // Toon "Levering Annuleren" knop alleen als levering nog niet verwerkt is en niet al geannuleerd
            if (!delivery.IsProcessed && delivery.Status != "Geannuleerd")
            {
                btnCancelDelivery.Visibility = Visibility.Visible;
            }
            
            // Als levering al verwerkt of geannuleerd is, maak velden read-only
            if (delivery.IsProcessed || delivery.Status == "Geannuleerd")
            {
                cmbDeliveryType.IsEnabled = false;
                txtReferenceNumber.IsReadOnly = true;
                cmbSupplier.IsEnabled = false;
                cmbCustomer.IsEnabled = false;
                cmbVehicle.IsEnabled = false;
                dpExpectedDate.IsEnabled = false;
                txtNotes.IsReadOnly = true;
                cmbProduct.IsEnabled = false;
                txtQuantity.IsReadOnly = true;
                txtUnitPrice.IsReadOnly = true;
                btnAddItem.IsEnabled = false;
                btnRemoveItem.IsEnabled = false;
                btnSave.IsEnabled = false;
                btnCancelDelivery.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadComboBoxes()
        {
            // Alleen actieve leveranciers tonen
            var suppliers = _context.Suppliers
                .Where(s => !s.IsDeleted && s.Status == "Active")
                .OrderBy(s => s.SupplierName)
                .ToList();
            cmbSupplier.ItemsSource = suppliers;
            
            // Alleen actieve klanten tonen
            var customers = _context.Customers
                .Where(c => !c.IsDeleted && c.Status == "Active")
                .OrderBy(c => c.CustomerName)
                .ToList();
            cmbCustomer.ItemsSource = customers;
            
            // Alleen beschikbare voertuigen tonen
            var vehicles = _context.Vehicles
                .Where(v => !v.IsDeleted && v.IsAvailable)
                .OrderBy(v => v.LicensePlate)
                .ToList();
            cmbVehicle.ItemsSource = vehicles;
            
            if (suppliers.Any())
                cmbSupplier.SelectedIndex = 0;
            
            // Set initial visibility to Incoming
            UpdateVisibility("Incoming");
        }

        private void LoadProducts()
        {
            // Alleen actieve producten met voldoende voorraad tonen
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
            if (deliveryType == "Outgoing")
            {
                // Bereken hoeveel er al in de lijst zit van dit product
                int alreadyInList = _items.Where(i => i.ProductId == product.ProductId)
                                          .Sum(i => i.Quantity);
                int totalRequested = alreadyInList + quantity;

                if (totalRequested > product.StockQuantity)
                {
                    MessageBox.Show(
                        $"ONVOLDOENDE VOORRAAD!\n\n" +
                        $"Product: {product.ProductName}\n" +
                        $"Beschikbare voorraad: {product.StockQuantity}\n" +
                        $"Al in levering: {alreadyInList}\n" +
                        $"Probeer toe te voegen: {quantity}\n" +
                        $"Totaal gevraagd: {totalRequested}\n\n" +
                        $"Je kunt maximaal {product.StockQuantity - alreadyInList} stuks toevoegen.",
                        "Voorraad Fout",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Waarschuwing als het dicht bij de limiet komt
                if (totalRequested == product.StockQuantity)
                {
                    var result = MessageBox.Show(
                        $"LET OP!\n\n" +
                        $"Je gebruikt de HELE voorraad van '{product.ProductName}'.\n" +
                        $"Totaal: {totalRequested} van {product.StockQuantity} stuks\n\n" +
                        "Wil je doorgaan?",
                        "Voorraad Waarschuwing",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;
                }
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

            // Extra validatie voor outgoing deliveries - check voorraad
            if (deliveryType == "Outgoing" && !_wasProcessed)
            {
                var validationErrors = new System.Text.StringBuilder();
                
                // Groepeer items per product en check totale hoeveelheid
                var productGroups = _items.GroupBy(i => i.ProductId);
                foreach (var group in productGroups)
                {
                    var product = _context.Products.Find(group.Key);
                    if (product != null)
                    {
                        int totalQuantity = group.Sum(i => i.Quantity);
                        
                        if (product.StockQuantity < totalQuantity)
                        {
                            validationErrors.AppendLine(
                                $"- {product.ProductName}: voorraad {product.StockQuantity}, nodig {totalQuantity}");
                        }
                    }
                }

                if (validationErrors.Length > 0)
                {
                    MessageBox.Show(
                        "ONVOLDOENDE VOORRAAD!\n\n" +
                        "De volgende producten hebben onvoldoende voorraad:\n\n" + 
                        validationErrors.ToString() +
                        "\nDe levering kan niet worden opgeslagen.",
                        "Voorraad Validatie",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
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
            
            // Status automatisch bepalen
            if (!_isEditMode || !_wasProcessed)
            {
                Delivery.Status = "Gepland";
            }
            // Als het al verwerkt was (IsProcessed=true), blijft de status "Delivered"
            
            Delivery.TotalAmount = _items.Sum(i => i.Quantity * i.UnitPrice);
            Delivery.Notes = txtNotes.Text.Trim();

            if (!_isEditMode)
            {
                Delivery.CreatedDate = DateTime.Now;
            }

            // Store items with the delivery
            Delivery.DeliveryItems = _items.ToList();

            DialogResult = true;
            Close();
        }

        private void btnCancelDelivery_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Weet u zeker dat u levering '{Delivery.ReferenceNumber}' wilt annuleren?\n\n" +
                "Deze actie kan niet ongedaan worden gemaakt.",
                "Levering Annuleren",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Delivery.Status = "Geannuleerd";
                DialogResult = true;
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}