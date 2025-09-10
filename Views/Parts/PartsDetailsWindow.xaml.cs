using System;
using System.Windows;
using System.Windows.Media;

namespace KTruckGui
{
    public partial class PartDetailsWindow : Window
    {
        private Models.Part _part;

        public PartDetailsWindow(Models.Part part)
        {
            InitializeComponent();
            _part = part;
            LoadPartDetails();
        }

        private void LoadPartDetails()
        {
            // Populate fields with part data
            PartIdValue.Text = _part.ID.ToString();
            NameValue.Text = _part.Name;

            // Use only existing properties of your Part class
            // For demonstration, show placeholder values for missing properties
            PartNumberValue.Text = "EF-2023-87T"; // Placeholder
            CategoryValue.Text = "Filters"; // Placeholder
            PriceValue.Text = $"${_part.Price:F2}";
            QuantityValue.Text = _part.Quantity.ToString();
            MinQuantityValue.Text = "10"; // Placeholder
            LocationValue.Text = _part.Location;

            // Handle LastUpdated if it exists
            LastUpdatedValue.Text = DateTime.Now.ToShortDateString(); // Placeholder

            // Set status based on quantity
            if (_part.Quantity <= 0)
            {
                StatusValue.Text = "Out of Stock";
                StatusValue.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (_part.Quantity < 10) // Assuming 10 is min quantity
            {
                StatusValue.Text = "Low Stock";
                StatusValue.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                StatusValue.Text = "In Stock";
                StatusValue.Foreground = new SolidColorBrush(Colors.Blue);
            }

            // Supplier information - use placeholders
            SupplierValue.Text = "SuperTruck Parts Inc."; // Placeholder
            SupplierPartNumberValue.Text = "STP-87452"; // Placeholder
            LastOrderDateValue.Text = DateTime.Now.AddMonths(-1).ToShortDateString(); // Placeholder
            LeadTimeValue.Text = "5-7 days"; // Placeholder
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EditPart_Click(object sender, RoutedEventArgs e)
        {
            // Implement edit functionality
            System.Windows.MessageBox.Show("Edit functionality will be implemented here.", "Edit Part",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeletePart_Click(object sender, RoutedEventArgs e)
        {
            // Implement delete functionality
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to delete this part?",
                                                                   "Delete Part",
                                                                   MessageBoxButton.YesNo,
                                                                   MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                // Delete logic here
                this.Close();
            }
        }
    }
}