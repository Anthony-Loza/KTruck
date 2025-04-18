using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using KTruckGui.Data;
using KTruckGui.Models;

namespace KTruckGui
{
    /// <summary>
    /// Interaction logic for PartsForm.xaml with integrated Purchase Order functionality
    /// </summary>
    public partial class PartsForm : Window, INotifyPropertyChanged
    {
        // Data Access
        private readonly PartDataAccess _partDataAccess;

        // Models
        private Part _part;
        private ObservableCollection<PartPurchaseOrder> _purchaseOrders;

        // Edit Mode Flag
        private bool _isEditMode;

        // Property Changed Event
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Collection of purchase orders for the part
        /// </summary>
        public ObservableCollection<PartPurchaseOrder> PurchaseOrders
        {
            get => _purchaseOrders;
            set
            {
                _purchaseOrders = value;
                OnPropertyChanged(nameof(PurchaseOrders));
            }
        }

        /// <summary>
        /// Constructor for creating a new part
        /// </summary>
        public PartsForm()
        {
            InitializeComponent();
            _partDataAccess = new PartDataAccess();
            _purchaseOrders = new ObservableCollection<PartPurchaseOrder>();
            _isEditMode = false;

            DataContext = this;
            HeaderText.Text = "Add New Part";
            PartStatusText.Text = "Status: New";

            // Set defaults
            ActiveCheckBox.IsChecked = true;
            PODatePicker.SelectedDate = DateTime.Today;
            SetDefaultValues();
        }

        /// <summary>
        /// Constructor for editing an existing part
        /// </summary>
        /// <param name="part">The part to edit</param>
        public PartsForm(Part part)
        {
            InitializeComponent();
            _partDataAccess = new PartDataAccess();
            _purchaseOrders = new ObservableCollection<PartPurchaseOrder>();
            _isEditMode = true;
            _part = part;

            DataContext = this;
            HeaderText.Text = "Edit Part";
            PartStatusText.Text = "Status: Existing";

            PopulatePartFields(part);
            LoadPurchaseOrders();
            UpdateStockStatus();
        }

        /// <summary>
        /// Set default values for a new part
        /// </summary>
        private void SetDefaultValues()
        {
            // Generate next part ID based on highest current ID
            try
            {
                var parts = _partDataAccess.GetParts();
                if (parts.Any())
                {
                    int maxId = parts.Max(p => p.ID);
                    IDTextBox.Text = (maxId + 1).ToString();
                }
                else
                {
                    IDTextBox.Text = "1001";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error generating part ID: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IDTextBox.Text = "1001";
            }

            // Default values for other fields
            QuantityTextBox.Text = "0";
            MinStockTextBox.Text = "5";
            ReorderTextBox.Text = "10";

            // Generate a default PO number
            PONumberTextBox.Text = $"PO-{DateTime.Now.ToString("yyyyMMdd")}-{new Random().Next(1000, 9999)}";
        }

        /// <summary>
        /// Populate fields with part data
        /// </summary>
        /// <param name="part">The part to populate from</param>
        private void PopulatePartFields(Part part)
        {
            // Basic Information
            IDTextBox.Text = part.ID.ToString();
            NameTextBox.Text = part.Name;
            DescriptionTextBox.Text = part.Description;
            LocationTextBox.Text = part.Location;
            ManufacturerTextBox.Text = part.Manufacturer;

            // Pricing Information
            UnitPriceTextBox.Text = part.UnitPrice?.ToString("0.00");
            PriceTextBox.Text = part.Price?.ToString("0.00");
            MarketPriceTextBox.Text = part.MarketPrice?.ToString("0.00");

            // Inventory Management
            QuantityTextBox.Text = part.Quantity?.ToString();
            MinStockTextBox.Text = part.MinimumStockLevel?.ToString();
            ReorderTextBox.Text = part.ReorderQuantity?.ToString();

            // Supplier Information
            SupplierTextBox.Text = part.Supplier;
            VendorTextBox.Text = part.Vendor;

            // Status
            ActiveCheckBox.IsChecked = part.Active;

            // Generate a default PO number for editing mode
            PONumberTextBox.Text = $"PO-{DateTime.Now.ToString("yyyyMMdd")}-{part.ID}";
        }

        /// <summary>
        /// Load purchase orders for the part
        /// </summary>
        private void LoadPurchaseOrders()
        {
            try
            {
                if (_part != null)
                {
                    var purchaseOrders = _partDataAccess.GetPurchaseOrdersForPart(_part.ID);
                    PurchaseOrders.Clear();

                    foreach (var po in purchaseOrders)
                    {
                        // Add custom properties for display that aren't in the database model
                        po.Supplier = _part.Supplier; // Use the part's supplier for display
                        po.PONumber = $"PO-{po.DateAdded.ToString("yyyyMMdd")}-{_part.ID}";

                        PurchaseOrders.Add(po);
                    }

                    PurchaseOrdersDataGrid.ItemsSource = PurchaseOrders;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading purchase orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Update the stock status display based on current quantity vs. minimum level
        /// </summary>
        private void UpdateStockStatus()
        {
            if (_part != null && _part.Quantity.HasValue && _part.MinimumStockLevel.HasValue)
            {
                if (_part.Quantity.Value <= 0)
                {
                    StockStatusText.Text = "Out of stock! Immediate reorder required.";
                }
                else if (_part.Quantity.Value < _part.MinimumStockLevel.Value)
                {
                    StockStatusText.Text = $"Below minimum stock level! Only {_part.Quantity.Value} remaining.";
                }
                else
                {
                    StockStatusText.Text = $"In stock: {_part.Quantity.Value} units available";
                }
            }
            else
            {
                // For new parts or parts without quantity data
                StockStatusText.Text = "New part - no stock data available";
            }
        }

        /// <summary>
        /// Save button click handler
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate ID
                if (string.IsNullOrWhiteSpace(IDTextBox.Text))
                {
                    System.Windows.MessageBox.Show("Part ID is required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(IDTextBox.Text, out int partId))
                {
                    System.Windows.MessageBox.Show("Part ID must be a number", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate Name
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    System.Windows.MessageBox.Show("Part Name is required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate that ID is not duplicate for new parts
                if (!_isEditMode && _partDataAccess.GetParts().Any(p => p.ID == partId))
                {
                    System.Windows.MessageBox.Show($"Part ID {partId} already exists. Please use a different ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create or update part object
                var part = new Part
                {
                    ID = partId,
                    Name = NameTextBox.Text.Trim(),
                    Description = DescriptionTextBox.Text.Trim(),
                    Location = LocationTextBox.Text.Trim(),
                    Manufacturer = ManufacturerTextBox.Text.Trim(),

                    // Numeric values - parse with fallback to null
                    UnitPrice = decimal.TryParse(UnitPriceTextBox.Text, out decimal unitPrice) ? unitPrice : (decimal?)null,
                    Price = decimal.TryParse(PriceTextBox.Text, out decimal price) ? price : (decimal?)null,
                    MarketPrice = decimal.TryParse(MarketPriceTextBox.Text, out decimal marketPrice) ? marketPrice : (decimal?)null,
                    Quantity = int.TryParse(QuantityTextBox.Text, out int quantity) ? quantity : (int?)null,
                    MinimumStockLevel = int.TryParse(MinStockTextBox.Text, out int minStock) ? minStock : (int?)null,
                    ReorderQuantity = int.TryParse(ReorderTextBox.Text, out int reorderQty) ? reorderQty : (int?)null,

                    // Supplier info
                    Supplier = SupplierTextBox.Text.Trim(),
                    Vendor = VendorTextBox.Text.Trim(),

                    // Status
                    Active = ActiveCheckBox.IsChecked ?? true,

                    // Audit fields
                    DateAdded = _isEditMode ? _part.DateAdded : DateTime.Now,
                    LastUpdated = DateTime.Now
                };

                // Save to database
                if (_isEditMode)
                {
                    _partDataAccess.UpdatePart(part);
                    System.Windows.MessageBox.Show("Part updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _partDataAccess.AddPart(part);
                    System.Windows.MessageBox.Show("Part added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Ask user if they want to create a purchase order if stock is low
                if (part.Quantity.HasValue && part.MinimumStockLevel.HasValue &&
                    part.Quantity.Value < part.MinimumStockLevel.Value)
                {
                    var result = System.Windows.MessageBox.Show(
                        "Current stock is below minimum level. Would you like to create a purchase order now?",
                        "Low Stock",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Create a purchase order - you could open a separate window or use the built-in functionality
                        GeneratePurchaseOrder_Click(sender, e);
                    }
                }

                // Close the form
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving part: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Cancel button click handler
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Upload PDF purchase order
        /// </summary>
        private void UploadPDF_Click(object sender, RoutedEventArgs e)
        {
            // Require a part ID before uploading  
            if (!int.TryParse(IDTextBox.Text, out int partId))
            {
                System.Windows.MessageBox.Show("Please enter a valid Part ID first.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Save the part first if in new mode  
            if (!_isEditMode)
            {
                var result = System.Windows.MessageBox.Show(
                    "You need to save the part before adding purchase orders. Save now?",
                    "Save Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveButton_Click(sender, e);
                }
                else
                {
                    return;
                }
            }

            // Open file dialog  
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PDF Files|*.pdf",
                Title = "Select Purchase Order PDF"
            };

            if (openFileDialog.ShowDialog() == true) // Fix: Correctly compare the result of ShowDialog()  
            {
                try
                {
                    // Read the PDF file  
                    byte[] pdfData = File.ReadAllBytes(openFileDialog.FileName);
                    string fileName = Path.GetFileName(openFileDialog.FileName);

                    // Save to database  
                    _partDataAccess.AddPurchaseOrderPDF(partId, pdfData, fileName);

                    System.Windows.MessageBox.Show("Purchase Order uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh the purchase orders list  
                    LoadPurchaseOrders();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error uploading PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// View selected PDF purchase order
        /// </summary>
        private void ViewPDF_Click(object sender, RoutedEventArgs e)
        {
            if (PurchaseOrdersDataGrid.SelectedItem is PartPurchaseOrder po)
            {
                try
                {
                    // Retrieve the PDF data
                    byte[] pdfData = _partDataAccess.GetPurchaseOrderPDF(po.ID);

                    if (pdfData == null || pdfData.Length == 0)
                    {
                        System.Windows.MessageBox.Show("Could not retrieve PDF data for this purchase order.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Create a temporary file
                    string tempFilePath = Path.Combine(Path.GetTempPath(), po.Filename);
                    File.WriteAllBytes(tempFilePath, pdfData);

                    // Open with default PDF viewer
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = tempFilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error opening PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a purchase order to view.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Delete selected PDF purchase order
        /// </summary>
        private void DeletePDF_Click(object sender, RoutedEventArgs e)
        {
            if (PurchaseOrdersDataGrid.SelectedItem is PartPurchaseOrder po)
            {
                // Confirm deletion
                var result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to delete the purchase order {po.PONumber}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Delete from database
                        _partDataAccess.DeletePurchaseOrderPDF(po.ID);

                        System.Windows.MessageBox.Show("Purchase Order deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh the purchase orders list
                        LoadPurchaseOrders();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error deleting purchase order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a purchase order to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Generate a new purchase order
        /// </summary>
        private void GeneratePurchaseOrder_Click(object sender, RoutedEventArgs e)
        {
            // Validate that we have the necessary info for a PO
            if (string.IsNullOrWhiteSpace(SupplierTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please enter a supplier name first.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create a customized PO filename
            string poFileName = $"PO_{DateTime.Now.ToString("yyyyMMdd")}_{NameTextBox.Text.Replace(" ", "_")}.pdf";

            // This would typically open a Purchase Order generator or form
            System.Windows.MessageBox.Show(
                "This would generate a new purchase order PDF based on part details.\n\n" +
                $"Part: {NameTextBox.Text}\n" +
                $"Supplier: {SupplierTextBox.Text}\n" +
                $"Quantity to Order: {ReorderTextBox.Text}\n" +
                $"Unit Price: ${UnitPriceTextBox.Text}",
                "Generate Purchase Order",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // For demonstration - in a real implementation, this would create the actual PDF
            // and then call AddPurchaseOrderPDF with the generated file
        }

        /// <summary>
        /// Handle purchase order selection change
        /// </summary>
        private void PurchaseOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update UI based on selection
            var isPoSelected = PurchaseOrdersDataGrid.SelectedItem != null;
            // In a real implementation, you might update preview, enable/disable buttons, etc.
        }

        /// <summary>
        /// Notify that a property has changed
        /// </summary>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}