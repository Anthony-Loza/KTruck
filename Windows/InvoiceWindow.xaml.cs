using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using KTruckGui.Models;
using KTruckGui.Data;
using System.IO;
using QuestPDF.Fluent;
using System.Diagnostics;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;


namespace KTruckGui
{
    public partial class InvoiceWindow : Window, INotifyPropertyChanged
    {
        private InvoiceDataAccess invoiceDataAccess;
        private CustomerDataAccess customerDataAccess;
        private Invoice _selectedInvoice;
        public Invoice SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                if (_selectedInvoice != value)
                {
                    _selectedInvoice = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsEditable));
                    OnPropertyChanged(nameof(IsReadOnly));
                    UpdateUIStateForFinalized(); // No parameter needed
                }
            }
        }
        public ObservableCollection<Part> Parts { get; set; }

        public bool IsEditable => !SelectedInvoice?.Status.Equals("finalized", StringComparison.OrdinalIgnoreCase) ?? true;

        public bool IsReadOnly => SelectedInvoice?.Status.Equals("finalized", StringComparison.OrdinalIgnoreCase) ?? false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public InvoiceWindow()
        {
            InitializeComponent();
            DataContext = this;
            invoiceDataAccess = new InvoiceDataAccess();
            customerDataAccess = new CustomerDataAccess();
            var partDataAccess = new PartDataAccess();

            Parts = new ObservableCollection<Part>(partDataAccess.GetParts());
            LoadInvoiceData(); // Call method to load data when the window initializes
            LoadCustomers();
        }

        private void LoadInvoiceData()
        {
            try
            {
                List<Invoice> invoices = invoiceDataAccess.GetInvoices();
                InvoiceDataGrid.ItemsSource = invoices;
            }
            catch (Exception ex)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    $"Failed to load invoices. Error: {ex.Message}\nWould you like to retry?",
                    "Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    LoadInvoiceData(); // Retry loading the data
                }
            }
        }
        private void OnReloadButtonClicked(object sender, EventArgs e)
        {
            LoadInvoiceData(); // Refresh the DataGrid
            LoadCustomers();
            System.Windows.MessageBox.Show("Data refreshed successfully!", "Reload", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void LoadInvoiceItems(string invoiceId)
        {
            try
            {
                if (!string.IsNullOrEmpty(invoiceId))
                {
                    var items = invoiceDataAccess.GetInvoiceItems(invoiceId);
                    ItemsDataGrid.ItemsSource = items;
                }
                else
                {
                    ItemsDataGrid.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    $"Failed to load invoice items. Error: {ex.Message}\nWould you like to retry?",
                    "Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    LoadInvoiceItems(invoiceId); // Retry loading the items
                }
            }
        }


        private void InvoiceDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InvoiceDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                SelectedInvoice = selectedInvoice;
                LoadInvoiceItems(selectedInvoice.Id);
                UpdateInvoiceSummary(selectedInvoice.Id);
            }
            else
            {
                SelectedInvoice = null;
                // Clear the form if no invoice is selected
                DescriptionTextBox.Text = string.Empty;
                CustomerComboBox.SelectedValue = null;
                StatusComboBox.SelectedIndex = -1;
                ItemsDataGrid.ItemsSource = null;

                // Clear Subtotal, Tax, and Total
                SubtotalTextBlock.Text = "0.00";
                TaxTextBlock.Text = "0.00";
                TotalTextBlock.Text = "0.00";
            }
        }

        private void UpdateInvoiceSummary(string invoiceId)
        {
            try
            {
                // Get all items for the selected invoice
                var items = invoiceDataAccess.GetInvoiceItems(invoiceId);

                // Separate parts and labor items
                var partItems = items.Where(item => item.Type.Equals("part", StringComparison.OrdinalIgnoreCase)).ToList();
                var laborItems = items.Where(item => item.Type.Equals("labor", StringComparison.OrdinalIgnoreCase)).ToList();

                // Calculate subtotal for labor (straightforward sum)
                decimal laborSubtotal = (decimal)laborItems.Sum(item => item.Total);

                // Calculate subtotal and tax for parts
                decimal partTotal = (decimal)partItems.Sum(item => item.Total); // Total with tax
                decimal partPreTaxSubtotal = partTotal / 1.0825m; // Pre-tax subtotal
                decimal partTax = partTotal - partPreTaxSubtotal; // Tax amount

                // Overall calculations
                decimal subtotal = laborSubtotal + partPreTaxSubtotal; // Overall subtotal
                decimal total = subtotal + partTax; // Grand total

                // Update the   
                SubtotalTextBlock.Text = subtotal.ToString("F2");
                TaxTextBlock.Text = partTax.ToString("F2");
                TotalTextBlock.Text = total.ToString("F2");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to update invoice summary. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ItemsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InvoiceDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                UpdateInvoiceSummary(selectedInvoice.Id);
            }
        }

        private void LoadCustomers()
        {
            var customers = customerDataAccess.GetCustomers() ?? new List<Customer>();
            var customerList = customers.Select(c => new
            {
                FullName = $"{c.FName} {c.LName}", // Combine first and last name
                c.Id
            }).ToList();

            CustomerComboBox.ItemsSource = customerList;
            CustomerComboBox.DisplayMemberPath = "FullName"; // Display full name
            CustomerComboBox.SelectedValuePath = "Id"; // Use ID as the selected value
        }


        private void EditPDF_Click(object sender, RoutedEventArgs e)
        {
        }

        private void GenerateInvoicePDF_Click(object sender, RoutedEventArgs e)
        {
            if (InvoiceDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                try
                {
                    // Fetch the invoice items for the selected invoice
                    var allInvoiceItems = (List<InvoiceItem>)ItemsDataGrid.ItemsSource;

                    // Update the database for the "OnInvoice" column
                    foreach (var item in allInvoiceItems)
                    {
                        invoiceDataAccess.UpdateInvoiceItemOnInvoiceStatus(item.ItemId, item.OnInvoice);
                    }

                    // Filter items to include only those with OnInvoice checked
                    var filteredInvoiceItems = allInvoiceItems.Where(item => item.OnInvoice).ToList();

                    // Ensure filtered items are added to the invoice model
                    selectedInvoice.InvoiceItems = filteredInvoiceItems;

                    // Fetch the customer data
                    var fromCustomer = new Customer { Id = "000000000", Type = "employee", FName = "KTruck", LName = "Repair", Address = "1075S Beltline Rd.", City = "Grand Prairie", State = "TX", Email = "Sid@KTruckRepair.com", Number = "8173339431", Zip = "75052", Discount = "0", DriverLicense = "00000000" };

                    var forCustomer = customerDataAccess.GetCustomerById(selectedInvoice.BillTo);

                    if (fromCustomer == null || forCustomer == null)
                    {
                        System.Windows.MessageBox.Show("Customer details are missing. Cannot generate the invoice.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Generate and display the PDF with filtered items
                    var document = new Assets.InvoiceDocument(selectedInvoice, fromCustomer, forCustomer);
                    var tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Invoice_{selectedInvoice.Id}.pdf");
                    document.GeneratePdf(tempFilePath);

                    // Open the generated PDF
                    Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to generate PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select an invoice to generate the PDF.", "No Invoice Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
 


        private void CreateInvoice_Click(object sender, RoutedEventArgs e)
        {
            InvoiceForm invoiceForm = new InvoiceForm();
            invoiceForm.ShowDialog();
        }

        private void DeleteInvoice_Click(object sender, RoutedEventArgs e)
        {
            // Ensure an invoice is selected
            if (InvoiceDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                // Show a confirmation dialog
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to delete the invoice with ID: {selectedInvoice.Id}?",
                    "Delete Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                // If the user confirms, proceed to delete the invoice
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Delete the invoice and its associated items
                        invoiceDataAccess.DeleteInvoice(selectedInvoice.Id);

                        // Refresh the DataGrid
                        LoadInvoiceData();

                        // Show a success message
                        System.Windows.MessageBox.Show($"Invoice with ID {selectedInvoice.Id} has been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"An error occurred while deleting the invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select an invoice to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private bool HandleFinalizedStatus(string invoiceId)
        {
            try
            {
                // Get all invoice items for the selected invoice
                var items = invoiceDataAccess.GetInvoiceItems(invoiceId);

                // Filter items that are parts and are marked for inclusion in the invoice
                var partItems = items
                    .Where(item => item.Type.Equals("part", StringComparison.OrdinalIgnoreCase) && item.OnInvoice)
                    .ToList();

                if (!partItems.Any())
                {
                    System.Windows.MessageBox.Show("No parts in this invoice require quantity reduction.",
                                                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true; // No action needed, proceed with status update
                }

                // Create a summary of changes
                string promptMessage = "The following parts will have their quantities reduced:\n\n";
                var partDataAccess = new PartDataAccess();
                foreach (var item in partItems)
                {
                    var part = partDataAccess.GetPartById(item.PartId.Value);
                    if (part == null)
                    {
                        System.Windows.MessageBox.Show($"Part with ID {item.PartId.Value} not found. Skipping.",
                                                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    int newQuantity = (int)(part.Quantity - item.Quantity);
                    promptMessage += $"- {part.Name}: Reduce by {item.Quantity}, New Total: {newQuantity}\n";
                }

                // Add the irreversible warning to the message
                promptMessage += "\nWARNING: Finalizing this invoice is irreversible. Are you sure you want to proceed?";

                // Show confirmation dialog
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    promptMessage,
                    "Confirm Quantity Reduction",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Reduce quantities
                    foreach (var item in partItems)
                    {
                        var part = partDataAccess.GetPartById(item.PartId.Value);
                        if (part == null)
                        {
                            System.Windows.MessageBox.Show($"Part with ID {item.PartId.Value} not found. Skipping.",
                                                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        int newQuantity = (int)(part.Quantity - item.Quantity);
                        partDataAccess.UpdatePartQuantity(item.PartId.Value, newQuantity);
                    }

                    System.Windows.MessageBox.Show("Quantities have been updated successfully.",
                                                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true; // Operation successful, proceed with status update
                }
                else
                {
                    return false; // User declined, do not proceed with status update
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to handle finalized status. Error: {ex.Message}",
                                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false; // Error occurred, do not proceed with status update
            }
        }


        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedInvoice == null) return;

            if (SelectedInvoice.Status.Equals("finalized", StringComparison.OrdinalIgnoreCase))
            {
                System.Windows.MessageBox.Show("Finalized invoices cannot be edited.", "Action Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (sender is System.Windows.Controls.ComboBox comboBox &&
                comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string status = selectedItem.Content.ToString();
                SelectedInvoice.Status = status;
                invoiceDataAccess.UpdateInvoiceStatus(SelectedInvoice.Id, status);
                UpdateUIStateForFinalized(); // No parameter needed

                if (status.Equals("finalized", StringComparison.OrdinalIgnoreCase))
                {
                    bool proceed = HandleFinalizedStatus(SelectedInvoice.Id);
                    if (!proceed)
                    {
                        SelectedInvoice.Status = "in progress";
                        invoiceDataAccess.UpdateInvoiceStatus(SelectedInvoice.Id, "in progress");
                        UpdateUIStateForFinalized(); // No parameter needed
                    }
                }
            }
        }

        private void UpdateStatusComboBoxState(string status)
        {
            if (status.Equals("finalized", StringComparison.OrdinalIgnoreCase))
            {
                StatusComboBox.IsEnabled = false; // Disable the ComboBox
            }
            else
            {
                StatusComboBox.IsEnabled = true; // Enable the ComboBox
            }
        }




        private void CustomerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InvoiceDataGrid.SelectedItem is Invoice selectedInvoice &&
                CustomerComboBox.SelectedValue is string selectedCustomerId)
            {
                try
                {
                    selectedInvoice.BillTo = selectedCustomerId;
                    invoiceDataAccess.UpdateInvoiceCustomer(selectedInvoice.Id, selectedCustomerId);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to update customer. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedInvoice == null) return;

            if (InvoiceDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                try
                {
                    string newDescription = DescriptionTextBox.Text;
                    selectedInvoice.Description = newDescription;
                    invoiceDataAccess.UpdateInvoiceDescription(selectedInvoice.Id, newDescription);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to update description. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateUIStateForFinalized()
        {
            bool isFinalized = SelectedInvoice?.Status.Equals("finalized", StringComparison.OrdinalIgnoreCase) ?? false;

            // Disable or enable UI elements based on finalized status
            DescriptionTextBox.IsEnabled = !isFinalized;
            CustomerComboBox.IsEnabled = !isFinalized;
            StatusComboBox.IsEnabled = !isFinalized;
            ItemsDataGrid.IsReadOnly = isFinalized;

            // Disable buttons for add, edit, and delete actions
            // Example for buttons (if they exist in your UI):
            // AddItemButton.IsEnabled = !isFinalized;
            // EditItemButton.IsEnabled = !isFinalized;
            // DeleteItemButton.IsEnabled = !isFinalized;
        }

        private void GenerateEstimatePDF_Click(object sender, RoutedEventArgs e)
        {
            if (InvoiceDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                try
                {
                    // Fetch the invoice items for the selected invoice
                    var allInvoiceItems = (List<InvoiceItem>)ItemsDataGrid.ItemsSource;

                    // Update the database for the "OnInvoice" column
                    foreach (var item in allInvoiceItems)
                    {
                        invoiceDataAccess.UpdateInvoiceItemOnInvoiceStatus(item.ItemId, item.OnInvoice);
                    }

                    // Filter items to include only those NOT marked with "OnInvoice"
                    var filteredEstimateItems = allInvoiceItems.Where(item => !item.OnInvoice).ToList();

                    // Ensure filtered items are added to the invoice model
                    selectedInvoice.InvoiceItems = filteredEstimateItems;

                    // Fetch the customer data
                    var fromCustomer = new Customer
                    {
                        Id = "000000000",
                        Type = "employee",
                        FName = "KTruck",
                        LName = "Repair",
                        Address = "1075S Beltline Rd.",
                        City = "Grand Prairie",
                        State = "TX",
                        Email = "Sid@KTruckRepair.com",
                        Number = "8173339431",
                        Zip = "75052",
                        Discount = "0",
                        DriverLicense = "00000000"
                    };

                    var forCustomer = customerDataAccess.GetCustomerById(selectedInvoice.BillTo);

                    if (fromCustomer == null || forCustomer == null)
                    {
                        System.Windows.MessageBox.Show("Customer details are missing. Cannot generate the estimate.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Generate and display the PDF with filtered estimate items
                    var document = new Assets.InvoiceDocument(selectedInvoice, fromCustomer, forCustomer);
                    var tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Estimate_{selectedInvoice.Id}.pdf");
                    document.GeneratePdf(tempFilePath);

                    // Open the generated PDF
                    Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to generate estimate PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select an invoice to generate the estimate PDF.", "No Invoice Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsDataGrid.SelectedItem is InvoiceItem selectedItem)
            {
                try
                {
                    if (!string.IsNullOrEmpty(selectedItem.ItemId))
                    {
                        // Update the existing item using InvoiceDataAccess
                        invoiceDataAccess.UpdateInvoiceItem(selectedItem);
                        System.Windows.MessageBox.Show($"Item with ID {selectedItem.ItemId} has been updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Add a new item to the invoice using InvoiceDataAccess
                        invoiceDataAccess.AddInvoiceItem(selectedItem, SelectedInvoice.Id);
                        System.Windows.MessageBox.Show("New item has been added to the invoice.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    // Refresh the DataGrid to reflect changes
                    if (SelectedInvoice != null)
                    {
                        LoadInvoiceItems(SelectedInvoice.Id);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to save item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select an item to save.", "No Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsDataGrid.SelectedItem is InvoiceItem selectedItem)
            {
                // Confirm with the user before deletion
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to delete the item with ID: {selectedItem.ItemId}?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Delete the item using InvoiceDataAccess
                        invoiceDataAccess.DeleteInvoiceItem(selectedItem.ItemId);

                        // Show a success message
                        System.Windows.MessageBox.Show($"Item with ID {selectedItem.ItemId} has been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh the DataGrid to reflect the deletion
                        if (SelectedInvoice != null)
                        {
                            LoadInvoiceItems(SelectedInvoice.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Show an error message if deletion fails
                        System.Windows.MessageBox.Show($"Failed to delete item. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                // No item selected
                System.Windows.MessageBox.Show("Please select an item to delete.", "No Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
