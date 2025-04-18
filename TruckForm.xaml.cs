using System;
using System.Windows;
using System.Windows.Controls;
using KTruckGui.Data;
using KTruckGui.Models;

namespace KTruckGui
{
    /// <summary>
    /// Interaction logic for TruckForm.xaml
    /// </summary>
    public partial class TruckForm : Window
    {
        private Vehicle _vehicle;
        private VehicleDataAccess _vehicleData;
        private CustomerDataAccess _customerData;
        private bool _isEditMode = false;

        // Default constructor for new vehicle
        public TruckForm()
        {
            InitializeComponent();
            _vehicleData = new VehicleDataAccess();
            _customerData = new CustomerDataAccess();
            _vehicle = new Vehicle();
            _isEditMode = false;

            // Set default values
            _vehicle.Active = true;
            StatusComboBox.SelectedIndex = 0; // Set default to "Active"

            // Load customers for driver dropdown
            LoadCustomers();

            // Set window title
            this.Title = "Add New Truck";

            // Update UI
            DataContext = _vehicle;
        }

        // Constructor for editing existing vehicle
        public TruckForm(Vehicle vehicle)
        {
            InitializeComponent();
            _vehicleData = new VehicleDataAccess();
            _customerData = new CustomerDataAccess();
            _vehicle = vehicle;
            _isEditMode = true;

            // Load customers for driver dropdown
            LoadCustomers();

            // Set window title
            this.Title = "Edit Truck";

            // Update UI
            DataContext = _vehicle;

            // Set form fields from vehicle data
            PopulateFormFields();
        }

        private void LoadCustomers()
        {
            try
            {
                // Get all customers
                var customers = _customerData.GetCustomers();

                // Add an empty option at the beginning with all required properties
                customers.Insert(0, new Customer
                {
                    Id = "",
                    FName = "-- None --",
                    LName = "",
                    Type = "Unknown",
                    Number = "",
                    Email = "",
                    Address = "",
                    City = "",
                    State = "",
                    Zip = "",
                    Discount = "0",
                    DriverLicense = ""
                });

                // Set the data source for the driver dropdown
                DriverComboBox.ItemsSource = customers;

                // Use the Name property that combines first and last name
                DriverComboBox.DisplayMemberPath = "Name";
                DriverComboBox.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateFormFields()
        {
            // Truck information
            TruckIdTextBox.Text = _vehicle.Id;

            // Set Make in ComboBox
            foreach (ComboBoxItem item in MakeComboBox.Items)
            {
                if (item.Content.ToString() == _vehicle.Make)
                {
                    MakeComboBox.SelectedItem = item;
                    break;
                }
            }

            ModelTextBox.Text = _vehicle.Model;
            YearTextBox.Text = _vehicle.Year.ToString();
            VinTextBox.Text = _vehicle.Vin;
            LicensePlateTextBox.Text = _vehicle.LicensePlate;
            MileageTextBox.Text = _vehicle.Odometer.ToString();

            // Set status in ComboBox - assuming Active property maps to Status
            string status = _vehicle.Active ? "Active" : "Out of Service";
            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content.ToString() == status)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }

            // Optional dates - set if available
            if (_vehicle.RegistrationExpiration.HasValue)
                RegistrationExpirationDatePicker.SelectedDate = _vehicle.RegistrationExpiration.Value;

            if (_vehicle.LastServiceDate.HasValue)
                LastServiceDatePicker.SelectedDate = _vehicle.LastServiceDate.Value;

            // Set driver selection if AssignmentTo has a value
            if (!string.IsNullOrEmpty(_vehicle.AssignmentTo))
            {
                DriverComboBox.SelectedValue = _vehicle.AssignmentTo;

                // Try to get driver information if available
                Customer driver = _customerData.GetCustomerById(_vehicle.AssignmentTo);
                if (driver != null)
                {
                    DriverLicenseTextBox.Text = driver.DriverLicense;
                    ContactNumberTextBox.Text = driver.Number;
                    // No license expiration in Customer model, so we'll leave that empty
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get selected Make
                var selectedMakeItem = MakeComboBox.SelectedItem as ComboBoxItem;
                string make = selectedMakeItem?.Content.ToString() ?? string.Empty;

                // Get selected Status
                var selectedStatusItem = StatusComboBox.SelectedItem as ComboBoxItem;
                string status = selectedStatusItem?.Content.ToString() ?? "Active";

                // Update vehicle from form fields
                _vehicle.Make = make;
                _vehicle.Model = ModelTextBox.Text;

                // Parse numeric values
                if (int.TryParse(YearTextBox.Text, out int year))
                {
                    _vehicle.Year = year;
                }

                _vehicle.LicensePlate = LicensePlateTextBox.Text;
                _vehicle.Vin = VinTextBox.Text;

                if (decimal.TryParse(MileageTextBox.Text, out decimal odometer))
                {
                    _vehicle.Odometer = odometer;
                }

                // Set Active based on Status
                _vehicle.Active = (status == "Active" || status == "In Service");

                // Update dates if selected
                _vehicle.RegistrationExpiration = RegistrationExpirationDatePicker.SelectedDate;
                _vehicle.LastServiceDate = LastServiceDatePicker.SelectedDate;

                // Update driver assignment
                _vehicle.AssignmentTo = DriverComboBox.SelectedValue?.ToString();

                // Validate required fields
                if (string.IsNullOrWhiteSpace(_vehicle.Make) ||
                    string.IsNullOrWhiteSpace(_vehicle.Model) ||
                    _vehicle.Year <= 0)
                {
                    System.Windows.MessageBox.Show("Please enter make, model, and year.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Save vehicle
                if (_isEditMode)
                {
                    _vehicleData.UpdateVehicle(_vehicle);
                    System.Windows.MessageBox.Show("Truck updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _vehicleData.AddVehicle(_vehicle);
                    System.Windows.MessageBox.Show("Truck added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving truck: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}