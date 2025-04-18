using System;
using System.Windows;
using System.Windows.Controls;
using KTruckGui.Data;
using KTruckGui.Models;

namespace KTruckGui
{
    public partial class CustomerForm : Window
    {
        private CustomerDataAccess _customerDataAccess;
        private bool _isEditMode = false;
        private Customer _existingCustomer;

        public CustomerForm(Customer customer = null)
        {
            InitializeComponent();
            _customerDataAccess = new CustomerDataAccess();

            if (customer != null)
            {
                _isEditMode = true;
                _existingCustomer = customer;
                PopulateCustomerFields(customer);
                Title = "Edit Customer";
                Save_Button.Content = "Update";
            }
            else
            {
                Title = "New Customer";
                // Optionally set a default customer type when creating a new customer
                CustomerTypeInput.SelectedIndex = 0;
            }

            RefreshCustomerSearchBox();
        }

        private void PopulateCustomerFields(Customer customer)
        {
            FirstName_Input.Text = customer.FName ?? string.Empty;
            LastName_Input.Text = customer.LName ?? string.Empty;
            CustomerTypeInput.Text = customer.Type ?? string.Empty;
            Phone1Input.Text = customer.Number ?? string.Empty;
            EmailInput.Text = customer.Email ?? string.Empty;
            AddressInput.Text = customer.Address ?? string.Empty;
            CityInput.Text = customer.City ?? string.Empty;
            StateInput.Text = customer.State ?? string.Empty;
            ZipInput.Text = customer.Zip ?? string.Empty;
            LicenseInput.Text = customer.DriverLicense ?? string.Empty;
            DiscountInput.Text = customer.Discount ?? string.Empty;
        }

        private void RefreshCustomerSearchBox()
        {
            var customerNames = _customerDataAccess.GetCustomerNames();

            // Ensure "New Customer" appears only once at the top
            if (!customerNames.Contains("New Customer"))
            {
                customerNames.Insert(0, "New Customer");
            }

            CustomerSearchBox.ItemsSource = customerNames;
            CustomerSearchBox.SelectedItem = "New Customer"; // Default to "New Customer"
        }

        private void CustomerSearchBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomerSearchBox.SelectedItem is string selectedCustomerName)
            {
                // Handle "New Customer" selection
                if (selectedCustomerName == "New Customer")
                {
                    ClearCustomerFields();
                    return;
                }

                // Fetch the customer details based on the selected name
                var customer = _customerDataAccess.GetCustomers()
                    .FirstOrDefault(c => $"{c.FName} {c.LName}" == selectedCustomerName);

                if (customer != null)
                {
                    PopulateCustomerFields(customer);
                    _existingCustomer = customer;
                    _isEditMode = true;
                    Save_Button.Content = "Update";
                }
            }
        }

        private void ClearCustomerFields()
        {
            // Reset all input fields
            FirstName_Input.Text = string.Empty;
            LastName_Input.Text = string.Empty;
            CustomerTypeInput.SelectedIndex = 0;
            Phone1Input.Text = string.Empty;
            EmailInput.Text = string.Empty;
            AddressInput.Text = string.Empty;
            CityInput.Text = string.Empty;
            StateInput.Text = string.Empty;
            ZipInput.Text = string.Empty;
            LicenseInput.Text = string.Empty;
            DiscountInput.Text = string.Empty;

            _isEditMode = false;
            _existingCustomer = null;
            Save_Button.Content = "Save";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields  
            if (string.IsNullOrWhiteSpace(FirstName_Input.Text) ||
                string.IsNullOrWhiteSpace(LastName_Input.Text))
            {
                System.Windows.MessageBox.Show("First Name and Last Name are required.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Construct the customer object  
            Customer customer = new()
            {
                Id = _isEditMode && _existingCustomer != null ? _existingCustomer.Id : GenerateUniqueCustomerId(), // Ensure 'Id' is set  
                FName = FirstName_Input.Text.Trim(),
                LName = LastName_Input.Text.Trim(),
                Type = CustomerTypeInput.Text,
                Number = Phone1Input.Text.Trim(),
                Email = EmailInput.Text.Trim(),
                Address = AddressInput.Text.Trim(),
                City = CityInput.Text.Trim(),
                State = StateInput.Text.Trim(),
                Zip = ZipInput.Text.Trim(),
                DriverLicense = LicenseInput.Text.Trim(),
                Discount = DiscountInput.Text.Trim()
            };

            try
            {
                if (_isEditMode && _existingCustomer != null)
                {
                    // Update existing customer  
                    _customerDataAccess.UpdateCustomer(customer);
                    System.Windows.MessageBox.Show("Customer updated successfully.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    // Save new customer  
                    _customerDataAccess.SaveCustomer(customer);
                    System.Windows.MessageBox.Show("New customer added successfully.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                // Refresh the search box to reflect changes  
                RefreshCustomerSearchBox();

                // Close the window or reset for new entry  
                ClearCustomerFields();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string GenerateUniqueCustomerId()
        {
            // Get the current maximum customer ID and increment
            var customers = _customerDataAccess.GetCustomers();
            int maxId = 0;

            foreach (var customer in customers)
            {
                if (int.TryParse(customer.Id, out int currentId))
                {
                    maxId = Math.Max(maxId, currentId);
                }
            }

            return (maxId + 1).ToString("D6"); // Pad with zeros to 6 digits
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the window
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Close the window
            this.Close();
        }
    }
}