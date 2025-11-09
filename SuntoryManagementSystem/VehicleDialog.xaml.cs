using SuntoryManagementSystem.Models;
using System.Windows;
using System.Windows.Controls;

namespace SuntoryManagementSystem
{
    public partial class VehicleDialog : Window
    {
        public Vehicle Vehicle { get; private set; }
        private readonly bool _isEditMode;

        public VehicleDialog()
        {
            InitializeComponent();
            Title = "Nieuw Voertuig Toevoegen";
            _isEditMode = false;
            Vehicle = new Vehicle();
        }

        public VehicleDialog(Vehicle vehicle) : this()
        {
            Title = "Voertuig Wijzigen";
            _isEditMode = true;
            Vehicle = vehicle;
            LoadVehicleData();
        }

        private void LoadVehicleData()
        {
            txtLicensePlate.Text = Vehicle.LicensePlate;
            txtBrand.Text = Vehicle.Brand;
            txtModel.Text = Vehicle.Model;
            cmbVehicleType.Text = Vehicle.VehicleType;
            txtCapacity.Text = Vehicle.Capacity.ToString();
            txtNotes.Text = Vehicle.Notes;
            
            // Set beschikbaarheid ComboBox
            cmbAvailability.SelectedIndex = Vehicle.IsAvailable ? 0 : 1;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLicensePlate.Text))
            {
                MessageBox.Show("Kenteken is verplicht!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLicensePlate.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBrand.Text))
            {
                MessageBox.Show("Merk is verplicht!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtBrand.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtModel.Text))
            {
                MessageBox.Show("Model is verplicht!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtModel.Focus();
                return;
            }

            if (!int.TryParse(txtCapacity.Text, out int capacity) || capacity < 0)
            {
                MessageBox.Show("Voer een geldige capaciteit in!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCapacity.Focus();
                return;
            }

            Vehicle.LicensePlate = txtLicensePlate.Text.Trim();
            Vehicle.Brand = txtBrand.Text.Trim();
            Vehicle.Model = txtModel.Text.Trim();
            Vehicle.VehicleType = ((ComboBoxItem)cmbVehicleType.SelectedItem).Content.ToString()!;
            Vehicle.Capacity = capacity;
            
            // Get beschikbaarheid van ComboBox
            var selectedItem = (ComboBoxItem)cmbAvailability.SelectedItem;
            Vehicle.IsAvailable = bool.Parse(selectedItem.Tag.ToString()!);
            
            Vehicle.Notes = txtNotes.Text.Trim();

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