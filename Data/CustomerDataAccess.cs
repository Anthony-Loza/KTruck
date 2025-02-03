using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using KTruckGui.Models;

namespace KTruckGui.Data
{
    public class CustomerDataAccess
    {
        private readonly string? connectionString;

        public CustomerDataAccess()
        {
            connectionString = Environment.GetEnvironmentVariable("DieselShopDb");

            // If the connection string is not found, throw an exception
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string could not be found in environment variables. Please ensure the DieselShopDb environment variable is set.");
            }
        }

        public List<Customer> GetCustomers()
        {
            var customers = new List<Customer>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Id, FName, LName, Type, Number, Email, Address, City, State, Zip, Discount, DriverLicense FROM Customers";
                    var command = new SqlCommand(query, connection);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new Customer
                            {
                                Id = reader["Id"].ToString(),
                                FName = reader["FName"].ToString(),
                                LName = reader["LName"].ToString(),
                                Type = reader["Type"].ToString(),
                                Number = reader["Number"].ToString(),
                                Email = reader["Email"].ToString(),
                                Address = reader["Address"].ToString(),
                                City = reader["City"].ToString(),
                                State = reader["State"].ToString(),
                                Zip = reader["Zip"].ToString(),
                                Discount = reader["Discount"].ToString(),
                                DriverLicense = reader["DriverLicense"].ToString()
                            });
                        }
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);
            }
            return customers;
        }

        private static List<string> _customerNamesCache;
        public List<string> GetCustomerNames()
        {
            if (_customerNamesCache != null)
                return _customerNamesCache;

            _customerNamesCache = new List<string>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT CONCAT(FName, ' ', LName) AS FullName FROM Customers";
                    var command = new SqlCommand(query, connection);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _customerNamesCache.Add(reader["FullName"].ToString());
                        }
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);
            }
            return _customerNamesCache;
        }



        public void SaveCustomer(Customer customer)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Customers (Id, FName, LName, Type, Number, Email, Address, City, State, Zip, Discount, DriverLicense) VALUES (@Id, @FName, @LName, @Type, @Number, @Email, @Address, @City, @State, @Zip, @Discount, @DriverLicense)";
                    var command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@Id", customer.Id);
                    command.Parameters.AddWithValue("@FName", customer.FName);
                    command.Parameters.AddWithValue("@LName", customer.LName);
                    command.Parameters.AddWithValue("@Type", customer.Type);
                    command.Parameters.AddWithValue("@Number", customer.Number);
                    command.Parameters.AddWithValue("@Email", customer.Email);
                    command.Parameters.AddWithValue("@Address", customer.Address);
                    command.Parameters.AddWithValue("@City", customer.City);
                    command.Parameters.AddWithValue("@State", customer.State);
                    command.Parameters.AddWithValue("@Zip", customer.Zip);
                    command.Parameters.AddWithValue("@Discount", customer.Discount);
                    command.Parameters.AddWithValue("@DriverLicense", customer.DriverLicense);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                _customerNamesCache = null;
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);

            }

        }


        public void UpdateCustomer(Customer customer)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Customers SET FName = @FName, LName = @LName, Type = @Type, Number = @Number, Email = @Email, Address = @Address, City = @City, State = @State, Zip = @Zip, Discount = @Discount, DriverLicense = @DriverLicense WHERE Id = @Id";
                    var command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@Id", customer.Id);
                    command.Parameters.AddWithValue("@FName", customer.FName);
                    command.Parameters.AddWithValue("@LName", customer.LName);
                    command.Parameters.AddWithValue("@Type", customer.Type);
                    command.Parameters.AddWithValue("@Number", customer.Number);
                    command.Parameters.AddWithValue("@Email", customer.Email);
                    command.Parameters.AddWithValue("@Address", customer.Address);
                    command.Parameters.AddWithValue("@City", customer.City);
                    command.Parameters.AddWithValue("@State", customer.State);
                    command.Parameters.AddWithValue("@Zip", customer.Zip);
                    command.Parameters.AddWithValue("@Discount", customer.Discount);
                    command.Parameters.AddWithValue("@DriverLicense", customer.DriverLicense);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                _customerNamesCache = null;
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);

            }

        }

        public void DeleteCustomer(string customerId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string vehicleQuery = "UPDATE Vehicle SET AssignmentTo = NULL WHERE AssignmentTo = @Id";
                    var command1 = new SqlCommand(vehicleQuery, connection);
                    command1.Parameters.AddWithValue("@Id", customerId);
                    connection.Open();
                    command1.ExecuteNonQuery();

                    string invoiceQuery = "UPDATE Invoices SET BillTo = NULL WHERE BillTo = @Id";
                    var command2 = new SqlCommand(invoiceQuery, connection);
                    command2.Parameters.AddWithValue("@Id", customerId);
                    command2.ExecuteNonQuery();

                    string deleteQuery = "DELETE FROM Customers WHERE Id = @Id";
                    var command3 = new SqlCommand(deleteQuery, connection);
                    command3.Parameters.AddWithValue("@Id", customerId);
                    command3.ExecuteNonQuery();
                }
                _customerNamesCache = null;
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);
            }
        }

        public List<Vehicle> GetVehiclesByCustomerId(string customerId)
        {
            var vehicles = new List<Vehicle>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Id, AssignmentTo, VIN, Make, Model, Year, Odometer, LicensePlate, Active FROM Vehicle WHERE AssignmentTo = @CustomerId";
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vehicles.Add(new Vehicle
                            {
                                Id = reader["Id"].ToString(),
                                AssignmentTo = reader["AssignmentTo"].ToString(),
                                Vin = reader["VIN"].ToString(),
                                Make = reader["Make"].ToString(),
                                Model = reader["Model"].ToString(),
                                Year = Convert.ToInt32(reader["Year"]),
                                Odometer = Convert.ToDecimal(reader["Odometer"]),
                                LicensePlate = reader["LicensePlate"].ToString(),
                                Active = Convert.ToBoolean(reader["Active"]) // Convert BIT to bool
                            });
                        }
                    }
                }

            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);

            }

            return vehicles;
        }
        public Customer GetCustomerById(string customerId)
        {
            Customer customer = null;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Id, FName, LName, Type, Number, Email, Address, City, State, Zip, Discount, DriverLicense FROM Customers WHERE Id = @CustomerId";
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = new Customer
                            {
                                Id = reader["Id"].ToString(),
                                FName = reader["FName"].ToString(),
                                LName = reader["LName"].ToString(),
                                Type = reader["Type"].ToString(),
                                Number = reader["Number"].ToString(),
                                Email = reader["Email"].ToString(),
                                Address = reader["Address"].ToString(),
                                City = reader["City"].ToString(),
                                State = reader["State"].ToString(),
                                Zip = reader["Zip"].ToString(),
                                Discount = reader["Discount"].ToString(),
                                DriverLicense = reader["DriverLicense"].ToString()
                            };
                        }
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);

            }
            return customer;
        }

    }
}
