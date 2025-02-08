using System;
using System.Collections.Generic;

namespace KTruckGui.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Invoices = new HashSet<Invoice>();
            Vehicles = new HashSet<Vehicle>();
        }
        public Customer(string id, string type, string Fname, string Lname, string address, string city, string state, string email, string number, string zip, string discount, string driverLicense)
        {
            FName = Fname;
            LName = Lname;
            Address = address;
            City = city;
            State = state;
            Email = email;
            Number = number;
            Zip = zip;
            Discount = discount;
            DriverLicense = driverLicense;
            Id = id;
            Type = type;

        }
            
        public required string Id { get; set; }
        public required string Type { get; set; }
        public required string FName { get; set; }
        public required string LName { get; set; }
        public string Name => $"{FName} {LName}";
        public required string Number { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string Zip { get; set; }
        public required string Discount { get; set; }
        public required string DriverLicense { get; set; }

        public virtual ICollection<Invoice>? Invoices { get; set; }
        public virtual ICollection<Vehicle>? Vehicles { get; set; }
    }
}
