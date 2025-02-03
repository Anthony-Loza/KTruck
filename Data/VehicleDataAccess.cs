// VehicleDataAccess.cs

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;  // Updated to use Microsoft.Data.SqlClient
using Microsoft.Extensions.Configuration;

using System.Windows;
using KTruckGui.Models;

namespace KTruckGui.Data

{
    using VehicleModel = KTruckGui.Models.Vehicle;

    public class VehicleDataAccess
    {
        private readonly string? connectionString;

        public VehicleDataAccess()
        {
            connectionString = Environment.GetEnvironmentVariable("DieselShopDb");

            // If the connection string is not found, throw an exception
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string could not be found in environment variables. Please ensure the DieselShopDb environment variable is set.");
            }
        }

        public List<VehicleModel> GetVehicle()
        {
            List<VehicleModel> vehicle = new List<VehicleModel>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Vehicle";
                    SqlCommand command = new SqlCommand(query, connection)
                    {
                        CommandTimeout = 30
                    };
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var vehicles = new VehicleModel
                        {
                            Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? string.Empty : reader.GetString(reader.GetOrdinal("Id")),
                            AssignmentTo = reader.IsDBNull(reader.GetOrdinal("AssignmentTo")) ? string.Empty : reader.GetString(reader.GetOrdinal("AssignmentTo")),
                            Make = reader.IsDBNull(reader.GetOrdinal("Make")) ? string.Empty : reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.IsDBNull(reader.GetOrdinal("Model")) ? string.Empty : reader.GetString(reader.GetOrdinal("Model")),
                            Year = reader.IsDBNull(reader.GetOrdinal("Year")) ? 0 : reader.GetInt32(reader.GetOrdinal("Year")),
                            LicensePlate = reader.IsDBNull(reader.GetOrdinal("LicensePlate")) ? string.Empty : reader.GetString(reader.GetOrdinal("LicensePlate")),
                            Vin = reader.IsDBNull(reader.GetOrdinal("Vin")) ? string.Empty : reader.GetString(reader.GetOrdinal("Vin")),
                            Odometer = reader.IsDBNull(reader.GetOrdinal("Odometer")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Odometer")),
                            Active = reader.IsDBNull(reader.GetOrdinal("Vin")) 

                        };
                        vehicle.Add(vehicles);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error accessing database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return vehicle;
        }

        public void DeleteVehicle(string vehicleId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Vehicle WHERE Id = @Id";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", vehicleId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting vehicle: {ex.Message}");
            }
        }

        public void SaveVehicle(Vehicle vehicle)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Vehicle (AssignmentTo, Make, Model, Year, LicensePlate, Odometer, Vin, Active) VALUES (@AssignmentTo, @Make, @Model, @Year, @LicensePlate, @Odometer, @Vin, @Active)";
                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@AssignmentTo", vehicle.AssignmentTo); // Ensure the ID is included
                command.Parameters.AddWithValue("@Make", vehicle.Make);
                command.Parameters.AddWithValue("@Model", vehicle.Model);
                command.Parameters.AddWithValue("@Year", vehicle.Year);
                command.Parameters.AddWithValue("@LicensePlate", vehicle.LicensePlate);
                command.Parameters.AddWithValue("@Odometer", vehicle.Odometer);
                command.Parameters.AddWithValue("@Vin", vehicle.Vin);
                command.Parameters.AddWithValue("@Active", vehicle.Active);



                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateVehicle(Vehicle vehicle)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Vehicle " +
                                   "SET AssignmentTo = @AssignmentTo, Make = @Make, Model = @Model, Year = @Year, LicensePlate = @LicensePlate,Odometer = @Odometer, Vin = @Vin, Active = @Active" +
                                   "WHERE Id = @Id";
                    var command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@Id", vehicle.Id);
                    command.Parameters.AddWithValue("@AssignmentTo", vehicle.AssignmentTo);
                    command.Parameters.AddWithValue("@Make", vehicle.Make);
                    command.Parameters.AddWithValue("@Model", vehicle.Model);
                    command.Parameters.AddWithValue("@Year", vehicle.Year);
                    command.Parameters.AddWithValue("@LicensePlate", vehicle.LicensePlate);
                    command.Parameters.AddWithValue("@Odometer", vehicle.Odometer);
                    command.Parameters.AddWithValue("@Vin", vehicle.Vin);
                    command.Parameters.AddWithValue("@Active", vehicle.Active);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error Updating vehicle");

            }

        }


    }
}
