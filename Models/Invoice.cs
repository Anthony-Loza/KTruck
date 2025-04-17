using System;
using System.Collections.Generic;

namespace KTruckGui.Models
{
    public partial class Invoice
    {
        public decimal Total { get; set; }
        public List<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

        public Invoice()
        {
            InvoiceItems = new List<InvoiceItem>();
        }

        public string Id { get; set; }
        public string BillTo { get; set; }
        public string Description { get; set; }
        public DateTime  DateCreated { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }

        public virtual Customer BillToNavigation { get; set; }
    }
}
