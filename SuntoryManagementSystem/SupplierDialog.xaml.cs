using SuntoryManagementSystem.Models;
using System.Windows;
using System.Windows.Controls;

namespace SuntoryManagementSystem
{
    public partial class SupplierDialog : Window
    {
        public Supplier Supplier { get; private set; }
        private readonly bool _isEditMode;

        public SupplierDialog()
        {
            InitializeComponent();
            Title = "Nieuwe Leverancier Toevoegen";
            _isEditMode = false;
            Supplier = new Supplier();
        }

        public SupplierDialog(Supplier supplier) : this()
        {
            Title = "Leverancier Wijzigen";
            _isEditMode = true;
            Supplier = supplier;
            LoadSupplierData();
        }

        private void LoadSupplierData()
        {
            txtSupplierName.Text = Supplier.SupplierName;
            txtAddress.Text = Supplier.Address;
            txtPostalCode.Text = Supplier.PostalCode;
            txtCity.Text = Supplier.City;
            txtPhoneNumber.Text = Supplier.PhoneNumber;
            txtEmail.Text = Supplier.Email;
            txtContactPerson.Text = Supplier.ContactPerson;
            cmbStatus.SelectedIndex = Supplier.Status == "Active" ? 0 : 1;
            txtNotes.Text = Supplier.Notes;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSupplierName.Text))
            {
                MessageBox.Show("Leveranciersnaam is verplicht!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSupplierName.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Voer een geldig e-mailadres in!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return;
            }

            Supplier.SupplierName = txtSupplierName.Text.Trim();
            Supplier.Address = txtAddress.Text.Trim();
            Supplier.PostalCode = txtPostalCode.Text.Trim();
            Supplier.City = txtCity.Text.Trim();
            Supplier.PhoneNumber = txtPhoneNumber.Text.Trim();
            Supplier.Email = txtEmail.Text.Trim();
            Supplier.ContactPerson = txtContactPerson.Text.Trim();
            Supplier.Status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString()!;
            Supplier.Notes = txtNotes.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}