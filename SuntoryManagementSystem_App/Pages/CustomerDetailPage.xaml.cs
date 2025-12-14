using SuntoryManagementSystem_App.Data;
using SuntoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.Pages;

[QueryProperty(nameof(CustomerId), nameof(CustomerId))]
[QueryProperty(nameof(ViewMode), nameof(ViewMode))]
public partial class CustomerDetailPage : ContentPage
{
    private readonly LocalDbContext _context;
    private Customer? _customer;
    private string? _customerIdString;
    private bool _viewMode;
    private bool _isLoaded = false;
    
    public string? CustomerId
    {
        get => _customerIdString;
        set
        {
            _customerIdString = value;
            Debug.WriteLine($"CustomerDetailPage: CustomerId set to {value}");
            
            if (!_isLoaded)
            {
                _isLoaded = true;
                _ = LoadDataAsync();
            }
        }
    }
    
    public string? ViewMode
    {
        get => _viewMode.ToString();
        set
        {
            _viewMode = value?.ToLower() == "true";
            Debug.WriteLine($"CustomerDetailPage: ViewMode set to {_viewMode}");
        }
    }
    
    public CustomerDetailPage(LocalDbContext context)
    {
        InitializeComponent();
        _context = context;
    }
    
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        if (!_isLoaded)
        {
            _isLoaded = true;
            _ = LoadDataAsync();
        }
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            Debug.WriteLine($"LoadDataAsync: Starting... CustomerId={_customerIdString}");
            
            // Try parse CustomerId
            int? customerId = null;
            if (!string.IsNullOrEmpty(_customerIdString) && int.TryParse(_customerIdString, out int parsedId))
            {
                customerId = parsedId;
            }
            
            if (customerId.HasValue && customerId.Value > 0)
            {
                Debug.WriteLine($"LoadDataAsync: Edit mode - loading customer {customerId}");
                // Edit mode - load existing customer
                _customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId.Value);
                
                if (_customer != null)
                {
                    Debug.WriteLine($"LoadDataAsync: Found customer {_customer.CustomerName}");
                    BindCustomerToForm();
                    
                    if (_viewMode)
                    {
                        Title = "Klant Bekijken";
                        DisableAllFields();
                        SaveButton.IsVisible = false;
                    }
                    else
                    {
                        Title = "Klant Wijzigen";
                    }
                }
                else
                {
                    Debug.WriteLine($"LoadDataAsync: Customer {customerId} not found");
                    await DisplayAlert("Fout", "Klant niet gevonden", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            else
            {
                Debug.WriteLine("LoadDataAsync: Create mode - new customer");
                Title = "Nieuwe Klant";
                // Create mode - new customer
                _customer = new Customer
                {
                    Status = "Active",
                    CustomerType = "Retail",
                    CreatedDate = DateTime.Now,
                    CustomerName = string.Empty,
                    Address = string.Empty,
                    PostalCode = string.Empty,
                    City = string.Empty,
                    PhoneNumber = string.Empty,
                    Email = string.Empty,
                    ContactPerson = string.Empty,
                    Notes = string.Empty
                };
                
                // Set defaults
                CustomerTypePicker.SelectedIndex = 0; // Retail
                StatusPicker.SelectedIndex = 0; // Active
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadDataAsync ERROR: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"Kan gegevens niet laden: {ex.Message}", "OK");
        }
    }
    
    private void DisableAllFields()
    {
        CustomerNameEntry.IsReadOnly = true;
        ContactPersonEntry.IsReadOnly = true;
        CustomerTypePicker.IsEnabled = false;
        AddressEntry.IsReadOnly = true;
        PostalCodeEntry.IsReadOnly = true;
        CityEntry.IsReadOnly = true;
        PhoneNumberEntry.IsReadOnly = true;
        EmailEntry.IsReadOnly = true;
        StatusPicker.IsEnabled = false;
        NotesEditor.IsReadOnly = true;
    }
    
    private void BindCustomerToForm()
    {
        if (_customer == null) return;
        
        Debug.WriteLine($"BindCustomerToForm: Binding {_customer.CustomerName}");
        
        CustomerNameEntry.Text = _customer.CustomerName;
        ContactPersonEntry.Text = _customer.ContactPerson ?? string.Empty;
        CustomerTypePicker.SelectedItem = _customer.CustomerType;
        AddressEntry.Text = _customer.Address ?? string.Empty;
        PostalCodeEntry.Text = _customer.PostalCode ?? string.Empty;
        CityEntry.Text = _customer.City ?? string.Empty;
        PhoneNumberEntry.Text = _customer.PhoneNumber ?? string.Empty;
        EmailEntry.Text = _customer.Email ?? string.Empty;
        StatusPicker.SelectedItem = _customer.Status;
        NotesEditor.Text = _customer.Notes ?? string.Empty;
    }
    
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Validatie
            if (string.IsNullOrWhiteSpace(CustomerNameEntry.Text))
            {
                await DisplayAlert("Fout", "Bedrijfsnaam is verplicht", "OK");
                return;
            }
            
            if (CustomerTypePicker.SelectedItem == null)
            {
                await DisplayAlert("Fout", "Selecteer een type", "OK");
                return;
            }
            
            // Email validatie (indien ingevuld)
            if (!string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                if (!IsValidEmail(EmailEntry.Text))
                {
                    await DisplayAlert("Fout", "Voer een geldig e-mailadres in", "OK");
                    return;
                }
            }
            
            // Update customer properties
            if (_customer == null) return;
            
            _customer.CustomerName = CustomerNameEntry.Text.Trim();
            _customer.ContactPerson = ContactPersonEntry.Text?.Trim() ?? string.Empty;
            _customer.CustomerType = CustomerTypePicker.SelectedItem?.ToString() ?? "Retail";
            _customer.Address = AddressEntry.Text?.Trim() ?? string.Empty;
            _customer.PostalCode = PostalCodeEntry.Text?.Trim() ?? string.Empty;
            _customer.City = CityEntry.Text?.Trim() ?? string.Empty;
            _customer.PhoneNumber = PhoneNumberEntry.Text?.Trim() ?? string.Empty;
            _customer.Email = EmailEntry.Text?.Trim() ?? string.Empty;
            _customer.Status = StatusPicker.SelectedItem?.ToString() ?? "Active";
            _customer.Notes = NotesEditor.Text?.Trim() ?? string.Empty;
            
            bool isNewCustomer = string.IsNullOrEmpty(_customerIdString);
            
            // Save customer
            if (isNewCustomer)
            {
                // Create new
                _customer.CreatedDate = DateTime.Now;
                await _context.Customers.AddAsync(_customer);
                await _context.SaveChangesAsync();
                
                Debug.WriteLine($"Created new customer with ID: {_customer.CustomerId}");
            }
            else
            {
                // Update existing
                _context.Customers.Update(_customer);
                await _context.SaveChangesAsync();
                
                Debug.WriteLine($"Updated customer {_customer.CustomerId}");
            }
            
            await DisplayAlert("Succes", 
                isNewCustomer ? "Nieuwe klant aangemaakt!" : "Klant bijgewerkt!", 
                "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnSaveClicked ERROR: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"Fout bij opslaan: {ex.Message}", "OK");
        }
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
    
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
