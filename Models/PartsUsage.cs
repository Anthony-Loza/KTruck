using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTruckGui.Models
{
    public class PartsUsage
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public DateTime UsageDate { get; set; }
        public int? TechnicianId { get; set; }
        public string Notes { get; set; }
        public int ReturnedQuantity { get; set; }
        public string ReturnReason { get; set; }

        // Navigation properties
        public virtual Workorder Workorder { get; set; }
        public virtual Part Part { get; set; }
        public virtual Technician Technician { get; set; }
    }
}