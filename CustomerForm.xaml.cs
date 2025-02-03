using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KTruckGui.Data;
using KTruckGui.Models; // Use the correct namespace

namespace KTruckGui
{
    /// <summary>
    /// Interaction logic for CustomerForm.xaml
    /// </summary>
    public partial class CustomerForm : Window
    {
        private CustomerDataAccess _customerDataAccess;

        public CustomerForm(Customer? customer = null)
        {
            InitializeComponent();

            // Initialize CustomerDataAccess
            _customerDataAccess = new CustomerDataAccess();

            // Set the ComboBox items for types
            List<string> types = new List<string> { "customer", "employee", "driver", "mechanic", "sales Person", "other" };
            CustomerTypeInput.ItemsSource = types;

            // Load customer names for autocomplete
            RefreshCustomerSearchBox();

            // If a Customer object is passed, populate the fields
            if (customer != null)
            {
                PopulateCustomerFields(customer);
            }
        }

        private void CustomerSearchBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomerSearchBox.SelectedItem is string selectedCustomerName)
            {
                Console.WriteLine($"Selected Customer Name: {selectedCustomerName}");

                // Handle "New Customer" selection
                if (selectedCustomerName == "New Customer")
                {
                    ClearCustomerFields();
                    ID_Input.IsReadOnly = false;
                    return;
                }

                // Fetch the customer details based on the selected name
                var customer = _customerDataAccess.GetCustomers()
                    .FirstOrDefault(c => $"{c.FName} {c.LName}" == selectedCustomerName);
                if (customer != null)
                {
                    Console.WriteLine($"Customer Found: {customer.FName} {customer.LName}");
                    PopulateCustomerFields(customer);
                    ID_Input.IsReadOnly = true;
                }
                else
                {
                    Console.WriteLine("Customer not found.");
                }
            }
            else
            {
                Console.WriteLine("Selected item is not a string.");
            }
        }

        private void PopulateCustomerFields(Customer customer)
        {
            if (customer == null)
            {
                System.Windows.MessageBox.Show("Customer is null.");
                return;
            }

            // Populate fields with customer data
            ID_Input.Text = customer.Id ?? string.Empty;
            FirstName_Input.Text = customer.FName ?? string.Empty;
            LastName_Input.Text = customer.LName ?? string.Empty;
            if (CustomerTypeInput.ItemsSource.Cast<string>().Contains(customer.Type))
            {
                CustomerTypeInput.SelectedItem = customer.Type;
            }
            else
            {
                CustomerTypeInput.SelectedItem = null; // Optional: Clear the selection if the type is not valid
            }
            Phone1Input.Text = customer.Number ?? string.Empty;
            LicenseInput.Text = customer.DriverLicense ?? string.Empty;
            AddressInput.Text = customer.Address ?? string.Empty;
            CityInput.Text = customer.City ?? string.Empty;
            StateInput.Text = customer.State ?? string.Empty;
            ZipInput.Text = customer.Zip ?? string.Empty;
            DiscountInput.Text = customer.Discount ?? string.Empty;
            EmailInput.Text = customer.Email ?? string.Empty;

            // Bind vehicles to the grid
            var vehicles = _customerDataAccess.GetVehiclesByCustomerId(customer.Id);
            VehiclesOwnerGrid.ItemsSource = vehicles;
        }
        private void ClearCustomerFields()
        {
            // Clear all input fields for creating a new customer
            ID_Input.Text = string.Empty;
            ID_Input.IsReadOnly = false;
            FirstName_Input.Text = string.Empty;
            LastName_Input.Text = string.Empty;
            CustomerTypeInput.SelectedItem = null;
            Phone1Input.Text = string.Empty;
            LicenseInput.Text = string.Empty;
            AddressInput.Text = string.Empty;
            CityInput.Text = string.Empty;
            StateInput.Text = string.Empty;
            ZipInput.Text = string.Empty;
            DiscountInput.Text = string.Empty;
            EmailInput.Text = string.Empty;
            VehiclesOwnerGrid.ItemsSource = null;
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool isNewCustomer = CustomerSearchBox.SelectedItem?.ToString() == "New Customer";


            // Construct the customer object
            var customer = new Customer
            {
                Id = ID_Input.Text,
                FName = FirstName_Input.Text,
                LName = LastName_Input.Text,
                Type = CustomerTypeInput.SelectedItem?.ToString(),
                Email = EmailInput.Text,
                Number = Phone1Input.Text,
                DriverLicense = LicenseInput.Text,
                Address = AddressInput.Text,
                City = CityInput.Text,
                State = StateInput.Text,
                Zip = ZipInput.Text,
                Discount = DiscountInput.Text
            };

            // Validate required fields
            if (string.IsNullOrEmpty(customer.FName) || string.IsNullOrEmpty(customer.LName))
            {
                System.Windows.MessageBox.Show("Customer First and Last Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Save or update the customer
            try
            {
                if (isNewCustomer)
                {
                    _customerDataAccess.SaveCustomer(customer);
                    System.Windows.MessageBox.Show("New customer added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _customerDataAccess.UpdateCustomer(customer);
                    System.Windows.MessageBox.Show("Customer details updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                RefreshCustomerSearchBox();
                this.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Handle checkbox logic if required
        }

        private void StateInput_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ID_Input_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void VehiclesOwnerGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ZipInput_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
