namespace KTruckGui.Models
{
    public class Vehicle
    {
        public required string Id { get; set; }
        public string AssignmentTo { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public decimal? Odometer { get; set; } // Added property for Odometer
        public string Vin { get; set; } // Added property for Vin
        public bool Active { get; set; }

    }
}
