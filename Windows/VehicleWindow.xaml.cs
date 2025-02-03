using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls; // Add this using directive
using KTruckGui.Models; // Corrected namespace
using KTruckGui.Data;

namespace KTruckGui
{
    public partial class VehicleWindow : Window
    {
        private VehicleDataAccess vehicleDataAccess;
        private CustomerDataAccess customerDataAccess;
        private bool isEditing = false; // Flag to prevent recursion
        private ObservableCollection<Vehicle> vehicles;


        public VehicleWindow()
        {
            InitializeComponent();
            vehicleDataAccess = new VehicleDataAccess();
            customerDataAccess = new CustomerDataAccess();
            LoadVehicleData();
            LoadCustomers();
        }

        private void LoadVehicleData()
        {
            vehicles = new ObservableCollection<Vehicle>(vehicleDataAccess.GetVehicle());
            VehicleDataGrid.ItemsSource = vehicles;
        }


        private void VehiclesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input fields
            if (OwnerComboBox.SelectedValue == null ||
                string.IsNullOrWhiteSpace(MakeTextBox.Text) ||
                string.IsNullOrWhiteSpace(ModelTextBox.Text) ||
                string.IsNullOrWhiteSpace(YearTextBox.Text) ||
                string.IsNullOrWhiteSpace(LicensePlateTextBox.Text))
            {
                System.Windows.MessageBox.Show("All fields are required. Please fill in all the details.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ensure Year is numeric
            if (!int.TryParse(YearTextBox.Text, out int year))
            {
                System.Windows.MessageBox.Show("Year must be a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string vin = "101010"; // Replace this with the actual VIN input from a TextBox or source.
            if (vin.Length < 4)
            {
                System.Windows.MessageBox.Show("VIN must be at least 4 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string id = vin.Substring(vin.Length - 4);

            // Create a new Vehicle object
            var newVehicle = new Vehicle
            {
                Id = id,
                AssignmentTo = OwnerComboBox.SelectedValue.ToString(),
                Make = MakeTextBox.Text,
                Model = ModelTextBox.Text,
                Year = year,
                LicensePlate = LicensePlateTextBox.Text,
                Odometer = (decimal)4.5,
                Vin = vin
            };

            try
            {
                // Save the vehicle to the database
                vehicleDataAccess.SaveVehicle(newVehicle);

                // Refresh the vehicle list
                LoadVehicleData();

                // Clear the form
                ClearVehicleDetails();

                // Show success message
                System.Windows.MessageBox.Show("Vehicle saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Show error message
                System.Windows.MessageBox.Show($"An error occurred while saving the vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void VehicleDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            // Prevent recursion using a flag
            if (isEditing)
                return;

            try
            {
                isEditing = true; // Set the flag to prevent re-entry

                // Commit the edit operation
                VehicleDataGrid.CommitEdit();

                // Get the edited vehicle
                if (e.Row.Item is Vehicle editedVehicle)
                {
                    // Confirm the edit
                    MessageBoxResult result = System.Windows.MessageBox.Show(
                        "Do you want to save the changes to this vehicle?",
                        "Confirm Edit",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Update the specific vehicle in the database
                            vehicleDataAccess.UpdateVehicle(editedVehicle);

                            // Update only the edited row in the ObservableCollection
                            var index = vehicles.IndexOf(vehicles.FirstOrDefault(v => v.Id == editedVehicle.Id));
                            if (index >= 0)
                            {
                                vehicles[index] = editedVehicle; // Replace the vehicle in the ObservableCollection
                            }

                            System.Windows.MessageBox.Show("Vehicle updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"An error occurred while updating the vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        // Reload the ObservableCollection if the user cancels the edit
                        LoadVehicleData();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isEditing = false; // Reset the flag
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if a vehicle is selected in the data grid
            if (VehicleDataGrid.SelectedItem is Vehicle selectedVehicle)
            {
                // Ask for user confirmation
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to delete the vehicle with ID: {selectedVehicle.Id}?",
                    "Delete Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Delete the vehicle from the database
                        vehicleDataAccess.DeleteVehicle(selectedVehicle.Id);

                        // Refresh the data grid
                        LoadVehicleData();

                        // Clear the details section
                        ClearVehicleDetails();

                        System.Windows.MessageBox.Show("Vehicle deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"An error occurred while deleting the vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a vehicle to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Clear the fields after deletion
        private void ClearVehicleDetails()
        {
            OwnerComboBox.Text = string.Empty;
            MakeTextBox.Text = string.Empty;
            ModelTextBox.Text = string.Empty;
            YearTextBox.Text = string.Empty;
            LicensePlateTextBox.Text = string.Empty;
        }

        private void OwnerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OwnerComboBox.SelectedItem is Customer selectedCustomer)
            {
                System.Windows.MessageBox.Show($"Selected Customer: {selectedCustomer.FName}");
            }

        }

        private void LoadCustomers()
        {
            List<Customer> customers = customerDataAccess.GetCustomers() ?? new List<Customer>();
            OwnerComboBox.ItemsSource = customers;
            OwnerComboBox.DisplayMemberPath = "Name";
            OwnerComboBox.SelectedValuePath = "Id";
        }

        private void MakeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
