using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
// Fix ambiguous references by using fully qualified names
using MediaColor = System.Windows.Media.Color;
using MediaColors = System.Windows.Media.Colors;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using WPFMessageBox = System.Windows.MessageBox;
using KTruckGui.Data;
using KTruckGui.Models;

namespace KTruckGui
{
    /// <summary>
    /// Interaction logic for CustomerDetailsWindow.xaml
    /// </summary>
    public partial class CustomerDetailsWindow : Window
    {
        // Customer data
        private readonly Customer _customer;
        private readonly CustomerDataAccess _customerData;
        private readonly VehicleDataAccess _vehicleData;
        private readonly InvoiceDataAccess _invoiceData;
        private Vehicle _selectedVehicle;
        private Invoice _selectedInvoice;

        // Constructor that takes a customer parameter
        public CustomerDetailsWindow(Customer customer)
        {
            InitializeComponent();
            _customer = customer;
            _customerData = new CustomerDataAccess();
            _vehicleData = new VehicleDataAccess();
            _invoiceData = new InvoiceDataAccess();

            LoadCustomerDetails();
            LoadCustomerVehicles();
            LoadCustomerInvoices();
        }

        private void LoadCustomerDetails()
        {
            try
            {
                // Populate customer information fields
                CustomerIdValue.Text = _customer.Id;
                NameValue.Text = $"{_customer.FName} {_customer.LName}";
                TypeValue.Text = _customer.Type;
                PhoneValue.Text = _customer.Number;
                EmailValue.Text = _customer.Email;
                AddressValue.Text = _customer.Address;
                CityValue.Text = _customer.City;
                StateValue.Text = _customer.State;
                ZipValue.Text = _customer.Zip;
                DiscountValue.Text = _customer.Discount;
                DriverLicenseValue.Text = _customer.DriverLicense;

                // Set customer type badge color based on type
                SolidColorBrush typeBrush;
                switch (_customer.Type?.ToLower())
                {
                    case "personal":
                        typeBrush = new SolidColorBrush(MediaColors.Blue);
                        TypeBorder.Background = new SolidColorBrush(MediaColor.FromRgb(232, 246, 255)); // Light blue
                        break;
                    case "business":
                        typeBrush = new SolidColorBrush(MediaColors.Green);
                        TypeBorder.Background = new SolidColorBrush(MediaColor.FromRgb(225, 255, 225)); // Light green
                        break;
                    case "fleet":
                        typeBrush = new SolidColorBrush(MediaColors.Orange);
                        TypeBorder.Background = new SolidColorBrush(MediaColor.FromRgb(255, 243, 224)); // Light orange
                        break;
                    default:
                        typeBrush = new SolidColorBrush(MediaColors.Gray);
                        TypeBorder.Background = new SolidColorBrush(MediaColor.FromRgb(240, 240, 240)); // Light gray
                        break;
                }
                TypeValue.Foreground = typeBrush;
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show($"Error loading customer details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCustomerVehicles()
        {
            try
            {
                List<Vehicle> vehicles = _vehicleData.GetVehiclesByCustomerId(_customer.Id);
                VehiclesDataGrid.ItemsSource = vehicles;
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show($"Error loading customer vehicles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCustomerInvoices()
        {
            try
            {
                // Get all invoices
                List<Invoice> allInvoices = _invoiceData.GetInvoices();

                // Filter for customer's invoices
                List<Invoice> customerInvoices = allInvoices.FindAll(invoice =>
                    invoice.BillTo != null && invoice.BillTo.Equals(_customer.Id));

                // Sort invoices by date (newest first)
                customerInvoices.Sort((a, b) => b.DateCreated.CompareTo(a.DateCreated));

                // Calculate totals for invoices (this would be more accurate with actual line items)
                foreach (var invoice in customerInvoices)
                {
                    // Get invoice items
                    List<InvoiceItem> items = _invoiceData.GetInvoiceItems(invoice.Id);

                    // Calculate total
                    decimal total = 0;
                    foreach (var item in items)
                    {
                        // Fix for decimal? to decimal conversion
                        total += item.Total.HasValue ? item.Total.Value : 0m;
                    }

                    // Add total property (not in the model, using a dynamic property)
                    invoice.Total = total;
                }

                InvoicesDataGrid.ItemsSource = customerInvoices;
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show($"Error loading customer invoices: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open CustomerForm with the current customer for editing
                var customerForm = new CustomerForm(_customer);
                customerForm.ShowDialog();

                // Refresh customer details
                Customer refreshedCustomer = _customerData.GetCustomerById(_customer.Id);
                if (refreshedCustomer != null)
                {
                    // Update the UI with refreshed data
                    LoadCustomerDetails();
                }
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show($"Error editing customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            // Confirm deletion
            MessageBoxResult result = WPFMessageBox.Show(
                $"Are you sure you want to delete customer: {_customer.FName} {_customer.LName}?\n\nThis will also remove all customer references from vehicles and invoices.",
                "Delete Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Delete the customer
                    _customerData.DeleteCustomer(_customer.Id);

                    WPFMessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Close this window
                    this.Close();
                }
                catch (Exception ex)
                {
                    WPFMessageBox.Show($"Error deleting customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void VehiclesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VehiclesDataGrid.SelectedItem != null)
            {
                _selectedVehicle = (Vehicle)VehiclesDataGrid.SelectedItem;
            }
            else
            {
                _selectedVehicle = null;
            }
        }

        private void InvoicesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InvoicesDataGrid.SelectedItem != null)
            {
                _selectedInvoice = (Invoice)InvoicesDataGrid.SelectedItem;
            }
            else
            {
                _selectedInvoice = null;
            }
        }

        private void ViewVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVehicle != null)
            {
                try
                {
                    // Open TruckDetailsWindow with the selected vehicle
                    var truckDetailsWindow = new TruckDetailsWindow(_selectedVehicle);
                    truckDetailsWindow.ShowDialog();

                    // Refresh vehicles list
                    LoadCustomerVehicles();
                }
                catch (Exception ex)
                {
                    WPFMessageBox.Show($"Error viewing vehicle details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                WPFMessageBox.Show("Please select a vehicle to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInvoice != null)
            {
                try
                {
                    // Open InvoiceWindow and select the specific invoice
                    var invoiceWindow = new InvoiceWindow();

                    // Note: You would need to add a method in InvoiceWindow to select a specific invoice
                    // This is a placeholder - the actual implementation will depend on InvoiceWindow's capabilities

                    invoiceWindow.ShowDialog();

                    // Refresh invoices list
                    LoadCustomerInvoices();
                }
                catch (Exception ex)
                {
                    WPFMessageBox.Show($"Error viewing invoice details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                WPFMessageBox.Show("Please select an invoice to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CreateInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a new invoice
                var invoiceForm = new InvoiceForm();
                invoiceForm.ShowDialog();

                // Refresh invoices list
                LoadCustomerInvoices();
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show($"Error creating new invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}