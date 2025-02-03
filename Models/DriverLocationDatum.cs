using System;
using System.Collections.Generic;

namespace KTruckGui.Models
{
    public partial class DriverLocationDatum
    {
        public long UserId { get; set; }
        public string DriverCompanyId { get; set; } = string.Empty; // Fix applied here
        public string CurrentLocationId { get; set; } = string.Empty; // Fix applied here
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? LocatedAt { get; set; }
        public string Description { get; set; } = string.Empty; // Fix applied here
        public string CurrentVehicle { get; set; } = string.Empty; // Fix applied here
        public string FirstName { get; set; } = string.Empty; // Fix applied here
        public string LastName { get; set; } = string.Empty; // Fix applied here
        public string Username { get; set; } = string.Empty; // Fix applied here
        public string Email { get; set; } = string.Empty; // Fix applied here
        public string Status { get; set; } = string.Empty; // Fix applied here
        public string Role { get; set; } = string.Empty; // Fix applied here
        public bool? HasDispatch { get; set; }
        public long? CurrentVehicleId { get; set; }
        public string CurrentVehicleNumber { get; set; } = string.Empty;
        public string CurrentVehicleYear { get; set; } = string.Empty;
        public string CurrentVehicleMake { get; set; } = string.Empty;
        public string CurrentVehicleModel { get; set; } = string.Empty;
        public string CurrentVehicleVin { get; set; } = string.Empty;
        public string NextStop { get; set; } = string.Empty;
        public string NextStopStatus { get; set; } = string.Empty;
        public string AvailableCity { get; set; } = string.Empty;
        public string AvailableDateTime { get; set; } = string.Empty;
    }
}