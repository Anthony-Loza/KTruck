using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Windows;
using KTruckGui.Models;

namespace KTruckGui.Data
{
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

        public List<Vehicle> GetAllVehicles()
        {
            List<Vehicle> vehicles = new List<Vehicle>();

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
                        var vehicle = new Vehicle
                        {
                            // Use safe methods to read from reader
                            Id = reader.GetSafeString("Id"),
                            AssignmentTo = reader.GetSafeString("AssignmentTo"),
                            Make = reader.GetSafeString("Make"),
                            Model = reader.GetSafeString("Model"),
                            Year = reader.GetSafeInt32("Year"),
                            LicensePlate = reader.GetSafeString("LicensePlate"),
                            Vin = reader.GetSafeString("VIN"),
                            Odometer = reader.GetSafeDecimal("Odometer"),
                            Active = reader.GetSafeBool("Active")
                        };
                        vehicles.Add(vehicle);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error accessing database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return vehicles;
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

        public void AddVehicle(Vehicle vehicle)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Vehicle (AssignmentTo, Make, Model, Year, LicensePlate, VIN, Odometer, Active) 
                    VALUES (@AssignmentTo, @Make, @Model, @Year, @LicensePlate, @VIN, @Odometer, @Active)";
                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@AssignmentTo", (object)vehicle.AssignmentTo ?? DBNull.Value);
                command.Parameters.AddWithValue("@Make", (object)vehicle.Make ?? DBNull.Value);
                command.Parameters.AddWithValue("@Model", (object)vehicle.Model ?? DBNull.Value);
                command.Parameters.AddWithValue("@Year", vehicle.Year);
                command.Parameters.AddWithValue("@LicensePlate", (object)vehicle.LicensePlate ?? DBNull.Value);
                command.Parameters.AddWithValue("@VIN", (object)vehicle.Vin ?? DBNull.Value);
                command.Parameters.AddWithValue("@Odometer", (object)vehicle.Odometer ?? DBNull.Value);
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
                    string query = @"
                        UPDATE Vehicle 
                        SET AssignmentTo = @AssignmentTo, 
                            Make = @Make, 
                            Model = @Model, 
                            Year = @Year, 
                            LicensePlate = @LicensePlate,
                            VIN = @VIN, 
                            Odometer = @Odometer, 
                            Active = @Active
                        WHERE Id = @Id";
                    var command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@Id", vehicle.Id);
                    command.Parameters.AddWithValue("@AssignmentTo", (object)vehicle.AssignmentTo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Make", (object)vehicle.Make ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Model", (object)vehicle.Model ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Year", vehicle.Year);
                    command.Parameters.AddWithValue("@LicensePlate", (object)vehicle.LicensePlate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VIN", (object)vehicle.Vin ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Odometer", (object)vehicle.Odometer ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Active", vehicle.Active);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error updating vehicle: {ex.Message}");
            }
        }
    }

    // Extension methods for SqlDataReader to safely handle null values
    public static class SqlDataReaderExtensions
    {
        public static string GetSafeString(this SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }

        public static int GetSafeInt32(this SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
        }

        public static decimal GetSafeDecimal(this SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetDecimal(ordinal);
        }

        public static bool GetSafeBool(this SqlDataReader reader, string columnName)
        {
            int ordinal;
            try
            {
                ordinal = reader.GetOrdinal(columnName);
            }
            catch
            {
                return false;
            }
            return reader.IsDBNull(ordinal) ? false : reader.GetBoolean(ordinal);
        }
    }
}