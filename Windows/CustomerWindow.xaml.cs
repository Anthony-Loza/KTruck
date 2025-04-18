using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using KTruckGui.Data;
using KTruckGui.Models;

namespace KTruckGui
{
    public partial class CustomerWindow : Window
    {
        private CustomerDataAccess customerDataAccess;
        private Customer selectedCustomer;

        public Customer SelectedCustomer { get; private set;}

        public CustomerWindow()
        {
            InitializeComponent();
            customerDataAccess = new CustomerDataAccess();

            // Initialize buttons in disabled state
            EditCustomerButton.IsEnabled = false;
            DeleteCustomerButton.IsEnabled = false;
            ViewDetailsButton.IsEnabled = false;
            CustomerVehiclesButton.IsEnabled = false;

            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            try
            {
                List<Customer> customers = customerDataAccess.GetCustomers();
                CustomerDataGrid.ItemsSource = customers;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading customers: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CustomerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomerDataGrid.SelectedItem is Customer customer)
            {
                selectedCustomer = customer;
                SelectedCustomer = customer; // Set the public property

                // Update button states based on selection
                EditCustomerButton.IsEnabled = true;
                DeleteCustomerButton.IsEnabled = true;
                ViewDetailsButton.IsEnabled = true;
                CustomerVehiclesButton.IsEnabled = true;
            }
            else
            {
                selectedCustomer = null;
                SelectedCustomer = null; // Clear the public property

                // Disable buttons that require selection
                EditCustomerButton.IsEnabled = false;
                DeleteCustomerButton.IsEnabled = false;
                ViewDetailsButton.IsEnabled = false;
                CustomerVehiclesButton.IsEnabled = false;
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                // If search is empty, reset to show all customers
                LoadCustomerData();
                return;
            }

            try
            {
                List<Customer> allCustomers = customerDataAccess.GetCustomers();

                // Filter customers based on search term
                var filteredCustomers = allCustomers.Where(c =>
                    (c.FName?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.LName?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.Email?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.Number?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.Address?.ToLower().Contains(searchTerm) ?? false) ||
                    (c.City?.ToLower().Contains(searchTerm) ?? false)
                ).ToList();

                CustomerDataGrid.ItemsSource = filteredCustomers;

                if (filteredCustomers.Count == 0)
                {
                    System.Windows.MessageBox.Show("No customers found matching your search criteria.", "No Results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error searching customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var customerForm = new CustomerForm();
            customerForm.ShowDialog();
            LoadCustomerData(); // Refresh data after adding new customer
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCustomer == null)
            {
                System.Windows.MessageBox.Show("Please select a customer to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var customerForm = new CustomerForm(selectedCustomer);
                customerForm.ShowDialog();
                LoadCustomerData(); // Refresh data after editing
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error editing customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCustomer == null)
            {
                System.Windows.MessageBox.Show("Please select a customer to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete customer: {selectedCustomer.FName} {selectedCustomer.LName}?",
                "Delete Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    customerDataAccess.DeleteCustomer(selectedCustomer.Id);
                    LoadCustomerData(); // Refresh data after deletion
                    System.Windows.MessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error deleting customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCustomer == null)
            {
                System.Windows.MessageBox.Show("Please select a customer to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var detailsWindow = new CustomerDetailsWindow(selectedCustomer);
                detailsWindow.ShowDialog();

                // Refresh data after details window is closed
                LoadCustomerData();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error opening customer details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CustomerVehicles_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCustomer == null)
            {
                System.Windows.MessageBox.Show("Please select a customer to view vehicles.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Open the CustomerDetailsWindow with focus on the vehicles section
                var detailsWindow = new CustomerDetailsWindow(selectedCustomer);
                detailsWindow.ShowDialog();

                // Refresh data after returning
                LoadCustomerData();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error viewing customer vehicles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }


}