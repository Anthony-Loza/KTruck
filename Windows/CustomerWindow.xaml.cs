using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using KTruckGui.Data;
using KTruckGui.Models;


namespace KTruckGui
{
    public partial class CustomerWindow : Window
    {
        private CustomerDataAccess customerDataAccess;

        public CustomerWindow()
        {
            InitializeComponent();
            customerDataAccess = new CustomerDataAccess();
            LoadCustomerData();
        }


        private void LoadCustomerData()
        {
            List<Customer> customers = customerDataAccess.GetCustomers();
            CustomerDataGrid.ItemsSource = customers;
        }

        private void CustomerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ClearCustomerDetails()
        {
            NameTextBox.Text = string.Empty;
            TypeTextBox.Text = string.Empty;
            NumberTextBox.Text = string.Empty;
            EmailTextBox.Text = string.Empty;
            AddressTextBox.Text = string.Empty;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerDataGrid.SelectedItem is Customer selectedCustomer)
            {
                selectedCustomer.FName = NameTextBox.Text;
                selectedCustomer.Type = TypeTextBox.Text;
                selectedCustomer.Number = NumberTextBox.Text;
                selectedCustomer.Email = EmailTextBox.Text;
                selectedCustomer.Address = AddressTextBox.Text;
                selectedCustomer.City = CityTextBox.Text;
                selectedCustomer.State = StateTextBox.Text;
                selectedCustomer.Zip = ZipTextBox.Text;
                selectedCustomer.Discount = DiscountTextBox.Text;

                var customerForm = new CustomerForm(selectedCustomer); // Assuming CustomerWindow class exists
                customerForm.Show();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerDataGrid.SelectedItem is Customer selectedCustomer)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to delete customer: {selectedCustomer.FName} {selectedCustomer.LName}?",
                    "Delete Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    customerDataAccess.DeleteCustomer(selectedCustomer.Id);
                    LoadCustomerData();
                    ClearCustomerDetails();
                    System.Windows.MessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var customerForm = new CustomerForm();
            customerForm.Show();
        }
    }
}
