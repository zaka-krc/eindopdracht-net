using SuntoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SuntoryManagementSystem
{
    public partial class DeliveryDialog : Window
    {
        public Delivery Delivery { get; private set; }
        private readonly bool _isEditMode;
        private readonly SuntoryDbContext _context;
        private bool _wasProcessed; // Track of levering al was verwerkt

        public DeliveryDialog(SuntoryDbContext context)
        {
            InitializeComponent();
            Title = "Nieuwe Levering Toevoegen";
            _isEditMode = false;
            _context = context;
            Delivery = new Delivery();
            _wasProcessed = false;
            LoadComboBoxes();
            dpExpectedDate.SelectedDate = DateTime.Now.AddDays(7);
        }

        public DeliveryDialog(SuntoryDbContext context, Delivery delivery) : this(context)
        {
            Title = "Levering Wijzigen";
            _isEditMode = true;
            Delivery = delivery;
            _wasProcessed = delivery.IsProcessed; // Bewaar originele status
            LoadDeliveryData();
        }

        private void LoadComboBoxes()
        {
            var suppliers = _context.Suppliers.OrderBy(s => s.SupplierName).ToList();
            cmbSupplier.ItemsSource = suppliers;
            
            var vehicles = _context.Vehicles.OrderBy(v => v.LicensePlate).ToList();
            cmbVehicle.ItemsSource = vehicles;
            
            if (suppliers.Any())
                cmbSupplier.SelectedIndex = 0;
        }

        private void LoadDeliveryData()
        {
            txtReferenceNumber.Text = Delivery.ReferenceNumber;
            cmbSupplier.SelectedValue = Delivery.SupplierId;
            cmbVehicle.SelectedValue = Delivery.VehicleId;
            dpExpectedDate.SelectedDate = Delivery.ExpectedDeliveryDate;
            cmbStatus.Text = Delivery.Status;
            txtTotalAmount.Text = Delivery.TotalAmount.ToString("F2");
            chkIsProcessed.IsChecked = Delivery.IsProcessed;
            txtNotes.Text = Delivery.Notes;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReferenceNumber.Text))
            {
                MessageBox.Show("Referentienummer is verplicht!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtReferenceNumber.Focus();
                return;
            }

            if (cmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een leverancier!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbSupplier.Focus();
                return;
            }

            if (dpExpectedDate.SelectedDate == null)
            {
                MessageBox.Show("Selecteer een verwachte datum!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpExpectedDate.Focus();
                return;
            }

            if (!decimal.TryParse(txtTotalAmount.Text, out decimal totalAmount) || totalAmount < 0)
            {
                MessageBox.Show("Voer een geldig totaalbedrag in!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTotalAmount.Focus();
                return;
            }

            bool isNowProcessed = chkIsProcessed.IsChecked ?? false;

            // NIEUW: Als levering nu wordt gemarkeerd als verwerkt
            if (isNowProcessed && !_wasProcessed)
            {
                var result = MessageBox.Show(
                    "Weet u zeker dat u deze levering wilt verwerken?\n\n" +
                    "Dit zal automatisch:\n" +
                    "- De voorraad bijwerken voor alle producten in deze levering\n" +
                    "- StockAdjustment records aanmaken\n" +
                    "- StockAlerts resolven indien nodig",
                    "Levering Verwerken",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }

                try
                {
                    ProcessDelivery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij het verwerken van de levering:\n{ex.Message}", 
                        "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            Delivery.ReferenceNumber = txtReferenceNumber.Text.Trim();
            Delivery.SupplierId = (int)cmbSupplier.SelectedValue;
            Delivery.VehicleId = cmbVehicle.SelectedValue as int?;
            Delivery.ExpectedDeliveryDate = dpExpectedDate.SelectedDate.Value;
            Delivery.Status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString()!;
            Delivery.TotalAmount = totalAmount;
            Delivery.IsProcessed = isNowProcessed;
            Delivery.Notes = txtNotes.Text.Trim();

            if (Delivery.Status == "Delivered" && Delivery.ActualDeliveryDate == null)
            {
                Delivery.ActualDeliveryDate = DateTime.Now;
            }

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Verwerkt de levering door voorraad bij te werken en StockAdjustments te maken
        /// </summary>
        private void ProcessDelivery()
        {
            // Haal alle delivery items op voor deze levering
            var deliveryItems = _context.DeliveryItems
                .Include(di => di.Product)
                .Where(di => di.DeliveryId == Delivery.DeliveryId && !di.IsDeleted)
                .ToList();

            if (!deliveryItems.Any())
            {
                MessageBox.Show(
                    "Deze levering heeft geen items om te verwerken.\n" +
                    "Voeg eerst producten toe aan deze levering.",
                    "Geen Items",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                throw new InvalidOperationException("Geen delivery items gevonden");
            }

            foreach (var item in deliveryItems)
            {
                if (item.Product == null)
                    continue;

                // Bewaar oude voorraad
                int previousQuantity = item.Product.StockQuantity;
                int newQuantity = previousQuantity + item.Quantity;

                // Update product voorraad
                item.Product.StockQuantity = newQuantity;

                // Maak StockAdjustment
                var adjustment = new StockAdjustment
                {
                    ProductId = item.ProductId,
                    AdjustmentType = "Addition",
                    QuantityChange = item.Quantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = newQuantity,
                    Reason = $"Levering {Delivery.ReferenceNumber} verwerkt",
                    AdjustedBy = "Systeem - Levering verwerking",
                    AdjustmentDate = DateTime.Now
                };

                _context.StockAdjustments.Add(adjustment);

                // Markeer delivery item als verwerkt
                item.IsProcessed = true;

                // Check of we StockAlerts moeten resolven
                if (newQuantity >= item.Product.MinimumStock)
                {
                    ResolveStockAlertsForProduct(item.ProductId);
                }
            }

            // Update delivery status
            if (Delivery.Status == "In Transit" || Delivery.Status == "Pending")
            {
                Delivery.Status = "Delivered";
            }

            if (Delivery.ActualDeliveryDate == null)
            {
                Delivery.ActualDeliveryDate = DateTime.Now;
            }

            _context.SaveChanges();

            MessageBox.Show(
                $"Levering succesvol verwerkt!\n\n" +
                $"- {deliveryItems.Count} product(en) toegevoegd aan voorraad\n" +
                $"- {deliveryItems.Count} voorraad aanpassing(en) geregistreerd",
                "Succes",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Resolved actieve StockAlerts voor een specifiek product
        /// </summary>
        private void ResolveStockAlertsForProduct(int productId)
        {
            var activeAlerts = _context.StockAlerts
                .Where(sa => sa.ProductId == productId 
                    && sa.Status == "Active" 
                    && !sa.IsDeleted)
                .ToList();

            foreach (var alert in activeAlerts)
            {
                alert.Status = "Resolved";
                alert.ResolvedDate = DateTime.Now;
                alert.Notes += $" - Opgelost door levering {Delivery.ReferenceNumber}";
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}