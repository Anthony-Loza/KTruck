using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KTruckGui.Data;
using KTruckGui.Models;

namespace KTruckGui
{
    /// <summary>
    /// Interaction logic for PartsForm.xaml
    /// </summary>
    public partial class PartsForm : Window
    {
        private PartDataAccess partDataAccess;
        private Part _part;
        public PartsForm(Part part = null)
        {
            partDataAccess = new PartDataAccess();
            InitializeComponent();
            if (part != null)
            {
                _part = part;
                PopulateFields(part); // Populate fields with the selected part's data
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate ID
                if (string.IsNullOrWhiteSpace(IDTextBox.Text))
                {
                    System.Windows.MessageBox.Show("ID field is required. Please enter a valid ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(IDTextBox.Text, out var parsedId))
                {
                    System.Windows.MessageBox.Show("Invalid ID. Please enter a numeric value.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check for duplicate ID if adding a new part
                if (_part == null && partDataAccess.GetParts().Any(p => p.ID == parsedId))
                {
                    System.Windows.MessageBox.Show($"The ID {parsedId} is already in use. Please use a unique ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create or update the part
                var partToSave = new Part
                {
                    ID = parsedId,
                    Name = NameTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    Price = decimal.TryParse(PriceTextBox.Text, out var price) ? price : (decimal?)null,
                    Quantity = int.TryParse(QuantityTextBox.Text, out var quantity) ? quantity : (int?)null,
                    Vendor = VendorTextBox.Text,
                    MarketPrice = decimal.TryParse(MarketPriceTextBox.Text, out var marketPrice) ? marketPrice : (decimal?)null,
                    Manufacturer = ManufacturerTextBox.Text,
                    Supplier = SupplierTextBox.Text,
                    QuantityInStock = QuantityInStockTextBox.Text,
                    UnitPrice = decimal.TryParse(UnitPriceTextBox.Text, out var unitPrice) ? unitPrice : (decimal?)null,
                    Location = LocationTextBox.Text,
                    MinimumStockLevel = int.TryParse(MinStockTextBox.Text, out var minStock) ? minStock : (int?)null,
                    ReorderQuantity = int.TryParse(ReorderTextBox.Text, out var reorderQty) ? reorderQty : (int?)null,
                    Active = ActiveCheckBox.IsChecked ?? false,
                    DateAdded = _part?.DateAdded ?? DateTime.Now,
                    LastUpdated = DateTime.Now
                };

                if (_part != null)
                {
                    partDataAccess.UpdatePart(partToSave);
                    System.Windows.MessageBox.Show("Part updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    partDataAccess.AddPart(partToSave);
                    System.Windows.MessageBox.Show("Part added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ClearFormFields();
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ClearFormFields()
        {
            IDTextBox.Clear();
            NameTextBox.Clear();
            DescriptionTextBox.Clear();
            PriceTextBox.Clear();
            QuantityTextBox.Clear();
            VendorTextBox.Clear();
            MarketPriceTextBox.Clear();
            ManufacturerTextBox.Clear();
            SupplierTextBox.Clear();
            QuantityInStockTextBox.Clear();
            UnitPriceTextBox.Clear();
            LocationTextBox.Clear();
            MinStockTextBox.Clear();
            ReorderTextBox.Clear();
            ActiveCheckBox.IsChecked = false;
        }
        private void PopulateFields(Part part)
        {
            IDTextBox.Text = part.ID.ToString();
            NameTextBox.Text = part.Name;
            DescriptionTextBox.Text = part.Description;
            PriceTextBox.Text = part.Price?.ToString();
            QuantityTextBox.Text = part.Quantity?.ToString();
            VendorTextBox.Text = part.Vendor;
            MarketPriceTextBox.Text = part.MarketPrice?.ToString();
            ManufacturerTextBox.Text = part.Manufacturer;
            SupplierTextBox.Text = part.Supplier;
            QuantityInStockTextBox.Text = part.QuantityInStock;
            UnitPriceTextBox.Text = part.UnitPrice?.ToString();
            LocationTextBox.Text = part.Location;
            MinStockTextBox.Text = part.MinimumStockLevel?.ToString();
            ReorderTextBox.Text = part.ReorderQuantity?.ToString();
            ActiveCheckBox.IsChecked = part.Active;
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
