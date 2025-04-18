using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTruckGui.Models
{
    public class WorkOrderStatusHistory
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public string Status { get; set; }
        public DateTime StatusDate { get; set; } = DateTime.Now;
        public string ChangedByUserId { get; set; }
        public string Notes { get; set; }
        public Workorder Workorder { get; set; }
    }
}
