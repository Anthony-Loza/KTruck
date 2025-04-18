using System;
using System.Collections.Generic;

namespace KTruckGui.Models
{
    public partial class Workorder
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? VehicleId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public int? TechnicianId { get; set; }
        public string DiagnosticNotes { get; set; }
        public string RepairNotes { get; set; }
        public int? CurrentOdometer { get; set; }
        public string EstimateId { get; set; }
        public string InvoiceId { get; set; }

    }
}