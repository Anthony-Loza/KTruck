using System;
using System.Collections.Generic;

namespace KTruckGui.Models
{
    public partial class ServiceHistory
    {
        public required string Id { get; set; }
        public required string VehicleId { get; set; }
        public required DateTime? ServiceDate { get; set; }
        public required string Description { get; set; }
        public required string TechnicianNotes { get; set; }
        public required DateTime? NextServiceDate { get; set; }
    }
}
