using System;
using System.ComponentModel;

namespace KTruckGui.Models
{
    public partial class InvoiceItem : INotifyPropertyChanged
    {
        private string _type;
        private int? _partId;
        private int _quantity;
        private decimal _rate;

        public string ItemId { get; set; }
        public string InvoiceId { get; set; }

        public string Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                    OnPropertyChanged(nameof(IsPartEditable));
                    OnPropertyChanged(nameof(IsTaxable)); // Notify if taxable status changes

                    // Reset PartId if Type is not "part"
                    if (_type != null && !_type.Equals("part", StringComparison.OrdinalIgnoreCase))
                    {
                        PartId = null;
                    }
                }
            }
        }

        public int? PartId
        {
            get => _partId;
            set
            {
                if (_partId != value)
                {
                    _partId = value;
                    OnPropertyChanged(nameof(PartId));
                }
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(Total)); // Notify when Total changes
                }
            }
        }

        public decimal Rate
        {
            get => _rate;
            set
            {
                if (_rate != value)
                {
                    _rate = value;
                    OnPropertyChanged(nameof(Rate));
                    OnPropertyChanged(nameof(Total)); // Notify when Total changes
                }
            }
        }

        public decimal? Total { get; set; }

        public string? Description { get; set; }
        public bool OnInvoice { get; set; }

        public virtual Invoice Invoice { get; set; }
        public virtual Part Part { get; set; }

        // Read-only property to control whether PartId is editable
        public bool IsPartEditable => Type?.Equals("part", StringComparison.OrdinalIgnoreCase) == true;

        // Read-only property to indicate if the item is taxable    
        public bool IsTaxable => Type?.Equals("part", StringComparison.OrdinalIgnoreCase) == true;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
