using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTruckGui.Models
{
    public class Diagnostic
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public DateTime DiagnosticDate { get; set; } = DateTime.Now;
        public int? TechnicianId { get; set; }
        public string DiagnosticCode { get; set; }
        public string DiagnosticDescription { get; set; }
        public string Recommendations { get; set; }
        public Workorder Workorder { get; set; }
    }
}
