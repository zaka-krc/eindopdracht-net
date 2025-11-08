using SuntoryManagementSystem.Models;
using System;
using System.Windows;

namespace SuntoryManagementSystem
{
    public partial class CustomerDialog : Window
    {
        public Customer Customer { get; private set; }
        private readonly bool _isEditMode;

        public CustomerDialog()
        {
            InitializeComponent();
            Title = "Nieuwe Klant Toevoegen";
            _isEditMode = false;
            Customer = new Customer();
        }

        public CustomerDialog(Customer customer) : this()
        {
            Title = "Klant Wijzigen";
            _isEditMode = true;
            Customer = customer;
            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            txtCustomerName.Text = Customer.CustomerName;
            txtAddress.Text = Customer.Address;
            txtPostalCode.Text = Customer.PostalCode;
            txtCity.Text = Customer.City;
            txtPhoneNumber.Text = Customer.PhoneNumber;
            txtEmail.Text = Customer.Email;
            txtContactPerson.Text = Customer.ContactPerson;
            cmbCustomerType.Text = Customer.CustomerType;
            
            foreach (System.Windows.Controls.ComboBoxItem item in cmbStatus.Items)
            {
                if (item.Content.ToString() == Customer.Status)
                {
                    cmbStatus.SelectedItem = item;
                    break;
                }
            }
            
            txtNotes.Text = Customer.Notes;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Klantnaam is verplicht!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomerName.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                if (!IsValidEmail(txtEmail.Text))
                {
                    MessageBox.Show("Voer een geldig e-mailadres in!", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtEmail.Focus();
                    return;
                }
            }

            Customer.CustomerName = txtCustomerName.Text.Trim();
            Customer.Address = txtAddress.Text.Trim();
            Customer.PostalCode = txtPostalCode.Text.Trim();
            Customer.City = txtCity.Text.Trim();
            Customer.PhoneNumber = txtPhoneNumber.Text.Trim();
            Customer.Email = txtEmail.Text.Trim();
            Customer.ContactPerson = txtContactPerson.Text.Trim();
            Customer.CustomerType = ((System.Windows.Controls.ComboBoxItem)cmbCustomerType.SelectedItem).Content.ToString()!;
            Customer.Status = ((System.Windows.Controls.ComboBoxItem)cmbStatus.SelectedItem).Content.ToString()!;
            Customer.Notes = txtNotes.Text.Trim();

            if (!_isEditMode)
            {
                Customer.CreatedDate = DateTime.Now;
            }

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
