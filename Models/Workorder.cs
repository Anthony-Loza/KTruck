using System;
using System.Collections.Generic;

namespace KTruckGui.Models
{
    public partial class Workorder
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? VehicleId { get; set; }
        public required string Description { get; set; }
        public required string Status { get; set; }
    }
}
