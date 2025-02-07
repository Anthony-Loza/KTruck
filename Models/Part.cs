using System;
using System.Collections.Generic;
using Microsoft.Identity.Client;

namespace KTruckGui.Models
{
    public partial class Part
    {
        public Part()
        {
            InvoiceItems = new HashSet<InvoiceItem>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public string Vendor { get; set; }
        public decimal? MarketPrice { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string Supplier { get; set; }
        public string QuantityInStock { get; set; }
        public decimal? UnitPrice { get; set; }
        public string Location { get; set; }
        public int? MinimumStockLevel { get; set; }
        public int? ReorderQuantity { get; set; }
        public DateTime? DateAdded { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool? Active {  get; set; }
        public List<PartPurchaseOrder> PurchaseOrders { get; set; } = new();


        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }
    }
}
