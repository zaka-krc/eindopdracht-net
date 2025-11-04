using SuntoryManagementSystem.Models;
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

        public DeliveryDialog(SuntoryDbContext context)
        {
            InitializeComponent();
            Title = "Nieuwe Levering Toevoegen";
            _isEditMode = false;
            _context = context;
            Delivery = new Delivery();
            LoadComboBoxes();
            dpExpectedDate.SelectedDate = DateTime.Now.AddDays(7);
        }

        public DeliveryDialog(SuntoryDbContext context, Delivery delivery) : this(context)
        {
            Title = "Levering Wijzigen";
            _isEditMode = true;
            Delivery = delivery;
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

            Delivery.ReferenceNumber = txtReferenceNumber.Text.Trim();
            Delivery.SupplierId = (int)cmbSupplier.SelectedValue;
            Delivery.VehicleId = cmbVehicle.SelectedValue as int?;
            Delivery.ExpectedDeliveryDate = dpExpectedDate.SelectedDate.Value;
            Delivery.Status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString()!;
            Delivery.TotalAmount = totalAmount;
            Delivery.IsProcessed = chkIsProcessed.IsChecked ?? false;
            Delivery.Notes = txtNotes.Text.Trim();

            if (Delivery.Status == "Delivered" && Delivery.ActualDeliveryDate == null)
            {
                Delivery.ActualDeliveryDate = DateTime.Now;
            }

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