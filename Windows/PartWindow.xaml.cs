using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using KTruckGui.Data;
using KTruckGui.Models;

namespace KTruckGui
{
    public partial class PartsWindow : Window
    {
        private readonly PartDataAccess partDataAccess; // Data access instance

        public PartsWindow()
        {
            InitializeComponent();
            partDataAccess = new PartDataAccess(); // Initialize data access class
            LoadPartsData(); // Populate DataGrid
        }

        private void LoadPartsData()
        {
            try
            {
                PartsDataGrid.ItemsSource = null; // Clear current data
                PartsDataGrid.ItemsSource = partDataAccess.GetParts(); // Reload data from the database
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load parts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPart_Click(object sender, RoutedEventArgs e)
        {
            var partForm = new PartsForm();
            partForm.Show();
            LoadPartsData();
        }

        private void EditPart_Click(object sender, RoutedEventArgs e)
        {   
            // Ensure a part is selected
            if (PartsDataGrid.SelectedItem is Part selectedPart)
            {
                var partForm = new PartsForm(selectedPart); // Pass the selected part to the form
                partForm.Show();
                LoadPartsData();
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a part to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeletePart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected part from the DataGrid
                if (PartsDataGrid.SelectedItem is Part selectedPart)
                {
                    // Show a confirmation message box
                    var result = System.Windows.MessageBox.Show(
                        $"Are you sure you want to delete the part: {selectedPart.Name} (ID: {selectedPart.ID})?",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        // Delete the part using PartDataAccess
                        partDataAccess.DeletePart(selectedPart.ID);
                        System.Windows.MessageBox.Show("Part deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Reload the DataGrid
                        LoadPartsData();
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Please select a part to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to delete part: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (PartsDataGrid.SelectedItem is Part selectedPart)
            {
                System.Windows.MessageBox.Show(
                    $"Part Details:\n\n" +
                    $"Name: {selectedPart.Name}\n" +
                    $"Price: {selectedPart.Price}\n" +
                    $"Quantity: {selectedPart.Quantity}\n" +
                    $"Vendor: {selectedPart.Vendor}",
                    "Part Details",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a part to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchQuery = SearchBox.Text.Trim();
                var parts = partDataAccess.GetParts()
                    .Where(p => p.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                                p.Location.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                PartsDataGrid.ItemsSource = parts;

                if (!parts.Any())
                {
                    System.Windows.MessageBox.Show("No matching parts found.", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to search parts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add the missing event handler
        private void PartsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PartsDataGrid.SelectedItem is Part selectedPart)
            {
                System.Windows.MessageBox.Show($"Selected Part: {selectedPart.Name}, Quantity: {selectedPart.Quantity}");
            }
        }
    }
}
