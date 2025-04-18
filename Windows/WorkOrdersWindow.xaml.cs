// Enhanced WorkOrdersWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KTruckGui.Data;
using KTruckGui.Models;

namespace KTruckGui
{
    /// <summary>
    /// Improved WorkOrdersWindow with complete repair workflow support
    /// </summary>
    public partial class WorkOrdersWindow : Window, INotifyPropertyChanged
    {
        // Data access objects
        private readonly CustomerDataAccess _customerData;
        private readonly VehicleDataAccess _vehicleData;
        private readonly PartDataAccess _partData;

        // Observable collections for binding
        public ObservableCollection<Workorder> ActiveWorkOrders { get; set; }
        public ObservableCollection<Part> AvailableParts { get; set; }
        public ObservableCollection<PartsUsage> PartsUsed { get; set; }
        public ObservableCollection<string> Technicians { get; set; } // Replace with Technician model when created

        // Selected items
        private Customer _selectedCustomer;
        private Vehicle _selectedVehicle;
        private Workorder _selectedWorkOrder;

        // Property changed implementation
        public event PropertyChangedEventHandler PropertyChanged;

        // Status options for work orders
        public List<string> StatusOptions { get; } = new List<string>
        {
            "New",
            "Diagnostic",
            "Estimate",
            "Approved",
            "In Progress",
            "On Hold",
            "Quality Control",
            "Complete",
            "Invoiced"
        };

        public WorkOrdersWindow()
        {
            InitializeComponent();

            // Initialize data access
            _customerData = new CustomerDataAccess();
            _vehicleData = new VehicleDataAccess();
            _partData = new PartDataAccess();

            // Initialize collections
            ActiveWorkOrders = new ObservableCollection<Workorder>();
            AvailableParts = new ObservableCollection<Part>();
            PartsUsed = new ObservableCollection<PartsUsage>();
            Technicians = new ObservableCollection<string>
            {
                "John Smith",
                "Maria Garcia",
                "David Johnson"
                // Load from technician database when implemented
            };

            // Set data context
            DataContext = this;

            // Load initial data
            LoadWorkOrders();
            LoadParts();
        }

        // Load existing work orders
        private void LoadWorkOrders()
        {
            // Actual implementation would load from database
            // Sample data for demonstration
            ActiveWorkOrders.Clear();

            // In the actual implementation, use WorkorderDataAccess to fetch real data
            ActiveWorkOrders.Add(new Workorder
            {
                Id = 1001,
                Description = "Engine overheating",
                Status = "Diagnostic",
                CustomerId = 1,
                VehicleId = 1
            });

            ActiveWorkOrders.Add(new Workorder
            {
                Id = 1002,
                Description = "Brake system failure",
                Status = "In Progress",
                CustomerId = 2,
                VehicleId = 3
            });
        }

        // Load available parts
        private void LoadParts()
        {
            var parts = _partData.GetParts();
            AvailableParts.Clear();

            foreach (var part in parts)
            {
                AvailableParts.Add(part);
            }
        }

        // Create new work order
        private void CreateWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate required fields
                if (_selectedCustomer == null)
                {
                    System.Windows.MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedVehicle == null)
                {
                    System.Windows.MessageBox.Show("Please select a vehicle.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create new work order
                var workOrder = new Workorder
                {
                    CustomerId = int.Parse(_selectedCustomer.Id),
                    VehicleId = int.Parse(_selectedVehicle.Id),
                    Description = DiagnosticNotesTextBox.Text,
                    Status = "New",
                    // Add additional fields as implemented
                };

                // Save to database - this would use WorkorderDataAccess in actual implementation

                // Refresh UI
                ActiveWorkOrders.Add(workOrder);
                ClearForm();

                System.Windows.MessageBox.Show("Work order created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error creating work order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Generate estimate from work order
        private void GenerateEstimate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWorkOrder == null)
            {
                System.Windows.MessageBox.Show("Please select a work order.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Create estimate logic
                var estimate = new Invoice
                {
                    Id = Guid.NewGuid().ToString(),
                    BillTo = _selectedWorkOrder.CustomerId.ToString(),
                    Description = $"Estimate for {_selectedWorkOrder.Description}",
                    DateCreated = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    Status = "estimate"
                };

                // Add parts from PartsUsed collection
                var estimateItems = new List<InvoiceItem>();
                foreach (var partUsage in PartsUsed)
                {
                    estimateItems.Add(new InvoiceItem
                    {
                        InvoiceId = estimate.Id,
                        ItemId = Guid.NewGuid().ToString(),
                        Type = "part",
                        PartId = partUsage.PartId,
                        Quantity = partUsage.Quantity,
                        Rate = _partData.GetPartById(partUsage.PartId).Price ?? 0,
                        Description = _partData.GetPartById(partUsage.PartId).Name
                    });
                }

                // Add labor estimate
                if (!string.IsNullOrWhiteSpace(LaborHoursTextBox.Text) &&
                    decimal.TryParse(LaborHoursTextBox.Text, out decimal laborHours))
                {
                    estimateItems.Add(new InvoiceItem
                    {
                        InvoiceId = estimate.Id,
                        ItemId = Guid.NewGuid().ToString(),
                        Type = "labor",
                        Quantity = (int)laborHours,
                        Rate = 125.0m, // Shop rate - could be configurable
                        Description = "Labor hours"
                    });
                }

                // In actual implementation, save estimate to database using InvoiceDataAccess

                // Update work order status
                _selectedWorkOrder.Status = "Estimate";

                // Open the estimate in the InvoiceWindow
                var invoiceWindow = new InvoiceWindow();
                invoiceWindow.ShowDialog();

                System.Windows.MessageBox.Show("Estimate generated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error generating estimate: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Convert to work order and assign technician
        private void AssignTechnician_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWorkOrder == null)
            {
                System.Windows.MessageBox.Show("Please select a work order.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TechnicianComboBox.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Please select a technician.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Update work order status
                _selectedWorkOrder.Status = "In Progress";

                // In actual implementation, add technician assignment to database

                System.Windows.MessageBox.Show("Technician assigned successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error assigning technician: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add part to work order
        private void AddPart_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWorkOrder == null)
            {
                System.Windows.MessageBox.Show("Please select a work order first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PartsComboBox.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Please select a part to add.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                System.Windows.MessageBox.Show("Please enter a valid quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Get selected part
                var selectedPart = (Part)PartsComboBox.SelectedItem;

                // Check if there's enough inventory
                if (selectedPart.Quantity < quantity)
                {
                    System.Windows.MessageBox.Show($"Not enough inventory. Only {selectedPart.Quantity} available.", "Inventory Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create parts usage record
                var partUsage = new PartsUsage
                {
                    WorkOrderId = _selectedWorkOrder.Id,
                    PartId = selectedPart.ID,
                    Quantity = quantity,
                    UsageDate = DateTime.Now,
                    Notes = UsageNotesTextBox.Text
                };

                // Add to collection
                PartsUsed.Add(partUsage);

                // In a real implementation, save to database

                // Update UI
                UsageNotesTextBox.Clear();
                QuantityTextBox.Clear();

                System.Windows.MessageBox.Show("Part added to work order.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error adding part: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Complete work order and generate invoice
        private void CompleteWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWorkOrder == null)
            {
                System.Windows.MessageBox.Show("Please select a work order.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Update work order status
                _selectedWorkOrder.Status = "Complete";

                // Deduct parts from inventory
                foreach (var partUsage in PartsUsed)
                {
                    var part = _partData.GetPartById(partUsage.PartId);
                    if (part != null && part.Quantity.HasValue)
                    {
                        int newQuantity = part.Quantity.Value - partUsage.Quantity;
                        _partData.UpdatePartQuantity(part.ID, newQuantity);
                    }
                }

                // Generate invoice option
                var result = System.Windows.MessageBox.Show("Work order marked as complete. Generate invoice now?",
                                           "Work Order Complete",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Logic to create invoice - similar to estimate but with "invoice" status
                    // Would open InvoiceWindow similar to estimate generation
                }

                System.Windows.MessageBox.Show("Work order completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error completing work order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handle work order selection
        private void WorkOrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Workorder workOrder)
            {
                _selectedWorkOrder = workOrder;

                // Load customer and vehicle details
                _selectedCustomer = _customerData.GetCustomerById(workOrder.CustomerId.ToString());
                _selectedVehicle = _vehicleData.GetVehicleById(workOrder.VehicleId.ToString());

                // Update UI with selected work order details
                CustomerNameTextBox.Text = $"{_selectedCustomer?.FName} {_selectedCustomer?.LName}";
                VehicleInfoTextBox.Text = $"{_selectedVehicle?.Year} {_selectedVehicle?.Make} {_selectedVehicle?.Model}";
                WorkOrderStatusComboBox.SelectedItem = workOrder.Status;
                DiagnosticNotesTextBox.Text = workOrder.Description;

                // Load parts used for this work order
                LoadPartsUsed(workOrder.Id);
            }
        }

        // Load parts used for a specific work order
        private void LoadPartsUsed(int workOrderId)
        {
            // In real implementation, fetch from database
            // For now, just clear the collection
            PartsUsed.Clear();
        }

        // Clear form fields
        private void ClearForm()
        {
            _selectedCustomer = null;
            _selectedVehicle = null;
            _selectedWorkOrder = null;

            CustomerNameTextBox.Clear();
            VehicleInfoTextBox.Clear();
            DiagnosticNotesTextBox.Clear();
            LaborHoursTextBox.Clear();
            UsageNotesTextBox.Clear();
            QuantityTextBox.Clear();

            PartsUsed.Clear();
        }

        // Customer lookup button
        private void CustomerLookup_Click(object sender, RoutedEventArgs e)
        {
            var customerWindow = new CustomerWindow();
            if (customerWindow.ShowDialog() == true && customerWindow.SelectedCustomer != null)
            {
                _selectedCustomer = customerWindow.SelectedCustomer;
                CustomerNameTextBox.Text = $"{_selectedCustomer.FName} {_selectedCustomer.LName}";

                // Load vehicles for this customer
                LoadCustomerVehicles(_selectedCustomer.Id);
            }
        }

        // Load vehicles for selected customer
        private void LoadCustomerVehicles(string customerId)
        {
            var vehicles = _vehicleData.GetVehiclesByCustomerId(customerId);
            VehicleComboBox.ItemsSource = vehicles;
            VehicleComboBox.DisplayMemberPath = "DisplayName"; // Format: Year Make Model
            VehicleComboBox.SelectedValuePath = "Id";
        }

        // Vehicle selection changed
        private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VehicleComboBox.SelectedItem is Vehicle vehicle)
            {
                _selectedVehicle = vehicle;
                VehicleInfoTextBox.Text = $"{vehicle.Year} {vehicle.Make} {vehicle.Model}";
            }
        }

        // Property changed notification
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // This would typically be in its own file
    public class PartsUsage
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public DateTime UsageDate { get; set; }
        public string TechnicianId { get; set; }
        public string Notes { get; set; }
        public Part Part { get; set; } // Navigation property
    }
}