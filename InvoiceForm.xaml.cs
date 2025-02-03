using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for InvoiceForm.xaml
    /// </summary>
    public partial class InvoiceForm : Window
    {
        private readonly InvoiceDataAccess dataAccess;
        private readonly CustomerDataAccess customerDataAccess;
        private readonly PartDataAccess partDataAccess;
        public ObservableCollection<InvoiceItem> InvoiceItems { get; set; }
            
        public List<Part> Parts { get; }
        public System.Windows.Controls.ComboBox TypeComboBox { get; }

        public InvoiceForm()
        {
            InitializeComponent();

            // Register converter in resources
            var typeToEnableConverter = new KTruckGui.Assets.TypeToEnableConverter();
            this.Resources.Add("TypeToEnableConverter", typeToEnableConverter);

            // Initialize collection
            InvoiceItems = new ObservableCollection<InvoiceItem>();
            InvoiceItems.CollectionChanged += InvoiceItems_CollectionChanged;

            InvoiceItemsGrid.ItemsSource = InvoiceItems;
    
            // Initialize data access
            dataAccess = new InvoiceDataAccess();
            customerDataAccess = new CustomerDataAccess();
            partDataAccess = new PartDataAccess();

            Parts = partDataAccess.GetParts();
            PopulateCustomerNames();

            DataContext = this; // Set DataContext for bindings
        }

        private void PopulateCustomerNames()
        {
            try
            {
                var customers = customerDataAccess.GetCustomers();

                // Create a list of anonymous objects with FullName and Id
                var customerList = customers.Select(c => new
                {
                    FullName = $"{c.FName} {c.LName}",
                    c.Id
                }).ToList();

                BillToInput.ItemsSource = customerList; // Bind the ComboBox
                BillToInput.DisplayMemberPath = "FullName"; // Display customer names
                BillToInput.SelectedValuePath = "Id"; // Use customer IDs as the selected value
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred while loading customer names: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate BillTo selection
                if (BillToInput.SelectedValue == null)
                {
                    System.Windows.MessageBox.Show("Please select a customer in the 'Bill To' field.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate Description
                if (string.IsNullOrWhiteSpace(DescriptionInput.Text))
                {
                    System.Windows.MessageBox.Show("Please enter a description.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate Dates
                if (!DateCreatedInput.SelectedDate.HasValue)
                {
                    System.Windows.MessageBox.Show("Please select a valid 'Date Created'.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!DueDateInput.SelectedDate.HasValue)
                {
                    System.Windows.MessageBox.Show("Please select a valid 'Due Date'.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate Items
                var items = (ObservableCollection<InvoiceItem>)InvoiceItemsGrid.ItemsSource;
                if (items == null || !items.Any())
                {
                    System.Windows.MessageBox.Show("Please add at least one invoice item.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Step 1: Generate a unique 12-character ID for the invoice
                string invoiceId = GenerateUniqueId(12);

                // Step 2: Populate InvoiceItems with InvoiceId
                foreach (var item in items)
                {
                    item.InvoiceId = invoiceId;

                    // Ensure that 'PartId' is selected for 'Type' = "part"
                    if (item.Type?.Equals("part", StringComparison.OrdinalIgnoreCase) == true && !item.PartId.HasValue)
                    {
                        System.Windows.MessageBox.Show("Please select a Part ID for all items where Type is 'part'.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Step 3: Get invoice details
                string billTo = BillToInput.SelectedValue.ToString();
                string description = DescriptionInput.Text;
                DateTime dateCreated = DateCreatedInput.SelectedDate.Value;
                DateTime dueDate = DueDateInput.SelectedDate.Value;

                // Step 4: Create an Invoice object
                var invoice = new Invoice
                {
                    Id = invoiceId,
                    BillTo = billTo,
                    Description = description,
                    DateCreated = dateCreated,
                    DueDate = dueDate
                };

                // Step 5: Save to the database
                dataAccess.AddInvoiceWithItems(invoice, items.ToList());

                // Step 6: Show success message
                System.Windows.MessageBox.Show("Invoice and items saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Step 7: Clear the form
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private string GenerateUniqueId(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        private void ClearForm()
        {
            BillToInput.SelectedValue = null; // Reset the selected customer
            DescriptionInput.Text = string.Empty;
            DateCreatedInput.SelectedDate = null;
            DueDateInput.SelectedDate = null;
            ((ObservableCollection<InvoiceItem>)InvoiceItemsGrid.ItemsSource).Clear();
        }


        private void BillToInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void InvoiceItemsGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Header?.ToString() == "Part ID")
            {
                if (e.Row.Item is InvoiceItem invoiceItem)
                {
                    Console.WriteLine($"IsPartEditable: {invoiceItem.IsPartEditable}, Type: {invoiceItem.Type}");

                    if (!invoiceItem.IsPartEditable)
                    {
                        e.Cancel = true;
                        System.Windows.MessageBox.Show("Part ID is only editable if Type is set to 'part'.",
                                        "Edit Not Allowed",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void InvoiceItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (InvoiceItem newItem in e.NewItems)
                {
                    newItem.PropertyChanged += InvoiceItem_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (InvoiceItem oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= InvoiceItem_PropertyChanged;
                }
            }

            UpdateTotals();
        }

        private void InvoiceItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceItem.Quantity) ||
                e.PropertyName == nameof(InvoiceItem.Rate) ||
                e.PropertyName == nameof(InvoiceItem.Type))
            {
                UpdateTotals();
            }
        }
        private void UpdateTotals()
        {
            decimal subtotal = 0;
            decimal tax = 0;

            foreach (var item in InvoiceItems)
            {
                if (item.Total.HasValue)
                {
                    subtotal += item.Total.Value;

                    // Apply tax only to items of type "part"
                    if (item.Type?.Equals("part", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        tax += item.Total.Value * 0.0825m;
                    }
                }
            }

            decimal total = subtotal + tax;

            // Update the UI
            SubtotalInput.Text = subtotal.ToString("F2");
            TaxInput.Text = tax.ToString("F2");
            TotalInput.Text = total.ToString("F2");
        }




    }
}
