using System;
using System.Collections.Generic;
using System.Linq;
using KTruckGui.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace KTruckGui.Data
{
    public class PartDataAccess
    {
        private readonly string? connectionString;
        private List<Part> cachedParts; // Cached list of parts
        private bool isCacheStale; // Flag to determine if cache needs to be refreshed

        public PartDataAccess()
        {
            connectionString = Environment.GetEnvironmentVariable("DieselShopDb");

            // If the connection string is not found, throw an exception
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string could not be found in environment variables. Please ensure the DieselShopDb environment variable is set.");
            }

            cachedParts = new List<Part>();
            isCacheStale = true; // Initialize as stale so the cache is loaded on first access
        }
        public Part GetPartById(int partId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Parts WHERE Id = @PartId";
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@PartId", partId);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Part
                            {
                                ID = partId,
                                Name = reader["Name"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"])
                            };
                        }
                    }
                }

                throw new Exception($"Part with ID {partId} not found.");
            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to get part by ID {partId}. Error: {ex.Message}", ex);
            }
        }



        // Retrieve all parts (with caching)
        public List<Part> GetParts()
        {
            if (isCacheStale || cachedParts == null || !cachedParts.Any())
            {
                RefreshCache();
            }

            return cachedParts;
        }

        // Refresh the cached list of parts
        private void RefreshCache()
        {
            var parts = new List<Part>();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    const string query = @"
                    SELECT 
                        ID, Name, Price, Quantity, Vendor, MarketPrice, Manufacturer, Supplier, 
                        QuantityInStock, UnitPrice, Location, MinimumStockLevel, ReorderQuantity, 
                        DateAdded, LastUpdated, Active
                    FROM Parts";

                    var command = new SqlCommand(query, connection);

                    try
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                parts.Add(new Part
                                {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    Name = reader["Name"].ToString(),
                                    Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : (decimal?)null,
                                    Quantity = reader["Quantity"] != DBNull.Value ? Convert.ToInt32(reader["Quantity"]) : (int?)null,
                                    Vendor = reader["Vendor"].ToString(),
                                    MarketPrice = reader["MarketPrice"] != DBNull.Value ? Convert.ToDecimal(reader["MarketPrice"]) : (decimal?)null,
                                    Manufacturer = reader["Manufacturer"].ToString(),
                                    Supplier = reader["Supplier"].ToString(),
                                    QuantityInStock = reader["QuantityInStock"].ToString(),
                                    UnitPrice = reader["UnitPrice"] != DBNull.Value ? Convert.ToDecimal(reader["UnitPrice"]) : (decimal?)null,
                                    Location = reader["Location"].ToString(),
                                    MinimumStockLevel = reader["MinimumStockLevel"] != DBNull.Value ? Convert.ToInt32(reader["MinimumStockLevel"]) : (int?)null,
                                    ReorderQuantity = reader["ReorderQuantity"] != DBNull.Value ? Convert.ToInt32(reader["ReorderQuantity"]) : (int?)null,
                                    DateAdded = reader["DateAdded"] != DBNull.Value ? Convert.ToDateTime(reader["DateAdded"]) : (DateTime?)null,
                                    LastUpdated = reader["LastUpdated"] != DBNull.Value ? Convert.ToDateTime(reader["LastUpdated"]) : (DateTime?)null,
                                    Active = reader["Active"] != DBNull.Value ? Convert.ToBoolean(reader["Active"]) : (bool?)null
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception("An error occurred while retrieving parts: " + ex.Message);
                    }
                }
                cachedParts = parts;
                isCacheStale = false;
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);

            }
        }

        // Add a new part
        public void AddPart(Part part)
        {
            const string query = @"
                INSERT INTO Parts 
                (ID, Name, Price, Quantity, Vendor, MarketPrice, Description, Manufacturer, Supplier, QuantityInStock, 
                 UnitPrice, Location, MinimumStockLevel, ReorderQuantity, DateAdded, LastUpdated, Active) 
                VALUES 
                (@ID, @Name, @Price, @Quantity, @Vendor, @MarketPrice, @Description, @Manufacturer, @Supplier, @QuantityInStock, 
                 @UnitPrice, @Location, @MinimumStockLevel, @ReorderQuantity, @DateAdded, @LastUpdated, @Active)";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(query, connection);
                    BindParameters(command, part);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception("An error occurred while adding a part: " + ex.Message);
                    }
                }

                // Add the part to the cache
                cachedParts.Add(part);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);

            }
        }

        // Update an existing part
        public void UpdatePart(Part part)
        {
            const string query = @"
        UPDATE Parts 
        SET Name = @Name, Price = @Price, Quantity = @Quantity, Vendor = @Vendor, MarketPrice = @MarketPrice, 
            Description = @Description, Manufacturer = @Manufacturer, Supplier = @Supplier, 
            QuantityInStock = @QuantityInStock, UnitPrice = @UnitPrice, Location = @Location, 
            MinimumStockLevel = @MinimumStockLevel, ReorderQuantity = @ReorderQuantity, 
            LastUpdated = @LastUpdated, Active = @Active
        WHERE ID = @ID";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(query, connection);

                    // Clear and add parameters
                    BindParameters(command, part); // Ensure all parameters are added in BindParameters

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception("An error occurred while updating a part: " + ex.Message);
                    }
                }

                // Update the cache
                var existingPart = cachedParts.FirstOrDefault(p => p.ID == part.ID);
                if (existingPart != null)
                {
                    cachedParts.Remove(existingPart);
                    cachedParts.Add(part);
                }
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);
            }
        }



        // Delete a part by ID
        public void DeletePart(int partId)
        {
            const string checkDependenciesQuery = "SELECT COUNT(*) FROM InvoiceItems WHERE PartID = @PartID";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check for dependencies
                var checkCommand = new SqlCommand(checkDependenciesQuery, connection);
                checkCommand.Parameters.AddWithValue("@PartID", partId);
                var dependencyCount = (int)checkCommand.ExecuteScalar();

                if (dependencyCount > 0)
                {
                    throw new Exception($"Cannot delete part. It is referenced by {dependencyCount} invoice item(s).");
                }

                // Delete the part
                const string deletePartQuery = "DELETE FROM Parts WHERE ID = @ID";
                var deleteCommand = new SqlCommand(deletePartQuery, connection);
                deleteCommand.Parameters.AddWithValue("@ID", partId);
                deleteCommand.ExecuteNonQuery();
            }
        }
        public void UpdatePartQuantity(int partId, int newQuantity)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Parts SET Quantity = @Quantity WHERE Id = @PartId";
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Quantity", newQuantity);
                    command.Parameters.AddWithValue("@PartId", partId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to update part quantity for Part ID {partId}. Error: {ex.Message}", ex);
            }
        }




        private void BindParameters(SqlCommand command, Part part)
        {
            // Clear parameters to avoid duplication
            command.Parameters.Clear();

            command.Parameters.AddWithValue("@ID", part.ID); // Include @ID here
            command.Parameters.AddWithValue("@Name", part.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Price", part.Price ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Quantity", part.Quantity ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Vendor", part.Vendor ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MarketPrice", part.MarketPrice ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Description", part.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Manufacturer", part.Manufacturer ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Supplier", part.Supplier ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@QuantityInStock", part.QuantityInStock ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UnitPrice", part.UnitPrice ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Location", part.Location ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MinimumStockLevel", part.MinimumStockLevel ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ReorderQuantity", part.ReorderQuantity ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DateAdded", part.DateAdded ?? DateTime.Now);
            command.Parameters.AddWithValue("@LastUpdated", part.LastUpdated ?? DateTime.Now);
            command.Parameters.AddWithValue("@Active", part.Active ?? (object)DBNull.Value);
        }

    }
}
