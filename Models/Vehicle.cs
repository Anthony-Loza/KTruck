namespace KTruckGui.Models
{
    public class Vehicle
    {
        public string Id { get; set; } // Primary key, matches the expected property name in error logs
        public string AssignmentTo { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public decimal? Odometer { get; set; } // Added property for Odometer, matching error logs
        public string Vin { get; set; } // Renamed to match error logs
        public bool Active { get; set; } // Added Active property

        // Additional properties for vehicle details
        public DateTime? LastServiceDate { get; set; }
        public DateTime? RegistrationExpiration { get; set; }
        public string Status { get; set; }
        public int ID { get; internal set; }
        public int VehicleID { get; internal set; }
        public string? Owner { get; internal set; }
        public string? VIN { get; internal set; }
        public int Mileage { get; internal set; }

        // Constructor with default values
        public Vehicle()
        {
            Id = string.Empty;
            AssignmentTo = string.Empty;
            Make = string.Empty;
            Model = string.Empty;
            Year = DateTime.Now.Year;
            LicensePlate = string.Empty;
            Odometer = 0;
            Vin = string.Empty;
            Active = true;
            Status = "Active";
        }
    }
}