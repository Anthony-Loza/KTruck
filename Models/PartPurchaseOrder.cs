using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KTruckGui.Models
{
    /// <summary>
    /// Represents a purchase order for a part with extended properties for display
    /// </summary>
    public class PartPurchaseOrder : INotifyPropertyChanged
    {
        private int _id;
        private int _partID;
        private byte[] _purchaseOrderPDF;
        private DateTime _dateAdded;
        private string _filename;
        private string _supplier;
        private string _poNumber;
        private decimal _orderQuantity;
        private decimal _unitPrice;
        private decimal _totalAmount;
        private string _status;
        private DateTime? _deliveryDate;
        private string _notes;
        private DateTime dateAdded = DateTime.Now;
        private string status = "Pending";

        // Database fields
        public int ID
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public int PartID
        {
            get => _partID;
            set
            {
                if (_partID != value)
                {
                    _partID = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte[] PurchaseOrderPDF
        {
            get => _purchaseOrderPDF;
            set
            {
                _purchaseOrderPDF = value;
                OnPropertyChanged();
            }
        }

        public DateTime DateAdded
        {
            get => _dateAdded;
            set
            {
                if (_dateAdded != value)
                {
                    _dateAdded = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Filename
        {
            get => _filename;
            set
            {
                if (_filename != value)
                {
                    _filename = value;
                    OnPropertyChanged();
                }
            }
        }

        // Navigation property
        public Part Part { get; set; }

        // Extended properties for UI display
        public string Supplier
        {
            get => _supplier;
            set
            {
                if (_supplier != value)
                {
                    _supplier = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PONumber
        {
            get => _poNumber;
            set
            {
                if (_poNumber != value)
                {
                    _poNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal OrderQuantity
        {
            get => _orderQuantity;
            set
            {
                if (_orderQuantity != value)
                {
                    _orderQuantity = value;
                    CalculateTotalAmount();
                    OnPropertyChanged();
                }
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    CalculateTotalAmount();
                    OnPropertyChanged();
                }
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (_totalAmount != value)
                {
                    _totalAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime? DeliveryDate
        {
            get => _deliveryDate;
            set
            {
                if (_deliveryDate != value)
                {
                    _deliveryDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged();
                }
            }
        }

        // Methods
        private void CalculateTotalAmount()
        {
            TotalAmount = OrderQuantity * UnitPrice;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}