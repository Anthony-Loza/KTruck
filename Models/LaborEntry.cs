using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTruckGui.Models
{
    public class LaborEntry
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public int TechnicianId { get; set; }
        public DateTime LaborDate { get; set; }
        public decimal HoursWorked { get; set; }
        public string Description { get; set; }
        public decimal? BillableHours { get; set; }

        // Navigation properties
        public virtual Workorder Workorder { get; set; }
        public virtual Technician Technician { get; set; }
    }
}