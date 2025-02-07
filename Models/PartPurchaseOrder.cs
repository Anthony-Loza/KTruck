using System;
using System.ComponentModel;

namespace KTruckGui.Models
{
    public class PartPurchaseOrder
    {
        public int ID { get; set; }
        public int PartID { get; set; } // Foreign key to Parts table
        public byte[] PurchaseOrderPDF { get; set; } // PDF file stored as binary
        public DateTime DateAdded { get; set; } = DateTime.Now;

        // Navigation property
        public Part Part { get; set; }
        public string Filename {  get; set; } 
    }
}
