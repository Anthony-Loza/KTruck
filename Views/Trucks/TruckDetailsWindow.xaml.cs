using System;
using System.Windows;
using System.Windows.Media;
using KTruckGui.Models;
using KTruckGui.Data;

namespace KTruckGui
{
    public partial class TruckDetailsWindow : Window
    {
        private Vehicle _vehicle;
        private VehicleDataAccess _vehicleData;

        public TruckDetailsWindow(Vehicle vehicle)
        {
            InitializeComponent();
            _vehicle = vehicle;
            _vehicleData = new VehicleDataAccess();
            LoadVehicleDetails();
        }

        private void LoadVehicleDetails()
        {
            try
            {
                // Basic truck information - check if UI elements exist before assigning
                if (TruckIdValue != null)
                    TruckIdValue.Text = _vehicle.Id;

                if (MakeValue != null)
                    MakeValue.Text = _vehicle.Make;

                if (ModelValue != null)
                    ModelValue.Text = _vehicle.Model;

                if (YearValue != null)
                    YearValue.Text = _vehicle.Year.ToString();

                if (VINValue != null)
                    VINValue.Text = _vehicle.Vin ?? "N/A";

                if (LicensePlateValue != null)
                    LicensePlateValue.Text = _vehicle.LicensePlate;

                if (MileageValue != null)
                    MileageValue.Text = _vehicle.Odometer.HasValue ? string.Format("{0:n0} mi", _vehicle.Odometer) : "N/A";

                // Set status based on vehicle status
                string status = _vehicle.Status ?? (_vehicle.Active ? "Active" : "Out of Service");

                if (StatusValue != null)
                {
                    StatusValue.Text = status;

                    // Set status color based on status
                    SolidColorBrush statusColor;
                    switch (status.ToLower())
                    {
                        case "active":
                            statusColor = new SolidColorBrush(Colors.Blue);
                            break;
                        case "in service":
                            statusColor = new SolidColorBrush(Colors.Orange);
                            break;
                        case "out of service":
                            statusColor = new SolidColorBrush(Colors.Red);
                            break;
                        default:
                            statusColor = new SolidColorBrush(Colors.Gray);
                            break;
                    }
                    StatusValue.Foreground = statusColor;
                }

                // Set last service date and registration expiration
                if (LastServiceValue != null)
                    LastServiceValue.Text = _vehicle.LastServiceDate?.ToShortDateString() ?? "Not Available";

                if (RegistrationValue != null)
                    RegistrationValue.Text = _vehicle.RegistrationExpiration?.ToShortDateString() ?? "Not Available";

                // Driver information (placeholder data for now)
                if (DriverNameValue != null)
                    DriverNameValue.Text = "Michael Johnson"; // Replace with actual driver data

                if (DriverLicenseValue != null)
                    DriverLicenseValue.Text = "TX-4758923";

                if (LicenseExpiresValue != null)
                    LicenseExpiresValue.Text = "08/15/2026";

                if (ContactNumberValue != null)
                    ContactNumberValue.Text = "(512) 555-1234";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading vehicle details: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EditTruck_Click(object sender, RoutedEventArgs e)
        {
            // Open TruckForm in edit mode
            var truckForm = new TruckForm(_vehicle);
            if (truckForm.ShowDialog() == true)
            {
                // Refresh the details if the vehicle was edited
                LoadVehicleDetails();
            }
        }

        private void DeleteTruck_Click(object sender, RoutedEventArgs e)
        {
            // Confirm deletion
            MessageBoxResult result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete {_vehicle.Make} {_vehicle.Model} ({_vehicle.LicensePlate})?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Delete the vehicle - we pass the ID as a string, not int
                    _vehicleData.DeleteVehicle(_vehicle.Id);

                    // Close this window
                    this.Close();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error deleting truck: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ServiceLog_Click(object sender, RoutedEventArgs e)
        {
            // Open service log window or navigate to the Maintenance tab
            System.Windows.MessageBox.Show("Service log functionality will be implemented here.",
                "Service Log", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}