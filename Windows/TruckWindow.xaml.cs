using System.Windows;
using System.Windows.Controls;
using KTruckGui.Models;
using KTruckGui.Data;

namespace KTruckGui
{
    public partial class VehicleWindow : Window
    {
        private bool isEditMode = false;
        private Vehicle currentVehicle = null;
        private VehicleDataAccess vehicleData = new VehicleDataAccess();
        private List<Vehicle> vehicles;

        // Removed the incorrect property declaration for TrucksDataGrid
        // The DataGrid is already defined in the XAML file, so this property is unnecessary and causes ambiguity.

        public VehicleWindow()
        {
            InitializeComponent();
            LoadVehicles();
        }

        private void LoadVehicles()
        {
            vehicles = vehicleData.GetAllVehicles();
            TrucksDataGrid.ItemsSource = vehicles; // This now correctly refers to the DataGrid defined in XAML.
        }

        private void TrucksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TrucksDataGrid.SelectedItem != null)
            {
                currentVehicle = (Vehicle)TrucksDataGrid.SelectedItem;
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                // If search is empty, reset to show all vehicles
                TrucksDataGrid.ItemsSource = vehicles;
                return;
            }

            // Filter vehicles based on search term
            List<Vehicle> filteredVehicles = vehicles.FindAll(vehicle =>
                vehicle.Make.ToLower().Contains(searchTerm) ||
                vehicle.Model.ToLower().Contains(searchTerm) ||
                vehicle.LicensePlate.ToLower().Contains(searchTerm) ||
                vehicle.Year.ToString().Contains(searchTerm)
            );

            TrucksDataGrid.ItemsSource = filteredVehicles;
        }

        private void AddTruck_Click(object sender, RoutedEventArgs e)
        {
            // Open a new TruckForm window for adding a truck
            var truckForm = new TruckForm();
            if (truckForm.ShowDialog() == true)
            {
                // Refresh the data grid
                LoadVehicles();
            }
        }

        private void EditTruck_Click(object sender, RoutedEventArgs e)
        {
            if (TrucksDataGrid.SelectedItem != null)
            {
                Vehicle selectedVehicle = (Vehicle)TrucksDataGrid.SelectedItem;

                // Open the TruckForm with the selected vehicle data
                var truckForm = new TruckForm(selectedVehicle);
                if (truckForm.ShowDialog() == true)
                {
                    // Refresh the data grid
                    LoadVehicles();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a truck to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteTruck_Click(object sender, RoutedEventArgs e)
        {
            if (TrucksDataGrid.SelectedItem != null)
            {
                Vehicle selectedVehicle = (Vehicle)TrucksDataGrid.SelectedItem;

                // Confirm deletion
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to delete {selectedVehicle.Make} {selectedVehicle.Model} ({selectedVehicle.LicensePlate})?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    // Convert the integer ID to a string before passing it to DeleteVehicle
                    vehicleData.DeleteVehicle(selectedVehicle.ID.ToString());

                    // Refresh the data grid
                    LoadVehicles();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a truck to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var selectedTruck = TrucksDataGrid.SelectedItem as Vehicle;
            if (selectedTruck != null)
            {
                var detailsWindow = new TruckDetailsWindow(selectedTruck);
                detailsWindow.ShowDialog();
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a truck to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ServiceHistory_Click(object sender, RoutedEventArgs e)
        {
            if (TrucksDataGrid.SelectedItem != null)
            {
                Vehicle selectedVehicle = (Vehicle)TrucksDataGrid.SelectedItem;

                System.Windows.MessageBox.Show("Service history functionality will be implemented here.", "Service History", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a truck to view service history.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}