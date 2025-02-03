using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using KTruckGui.Models;

namespace KTruckGui.Data
{
    public class InvoiceDataAccess
    {
        private readonly string? connectionString;

        public InvoiceDataAccess()
        {
            connectionString = Environment.GetEnvironmentVariable("DieselShopDb");

            // If the connection string is not found, throw an exception
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string could not be found in environment variables. Please ensure the DieselShopDb environment variable is set.");
            }
        }

        public List<Invoice> GetInvoices()
        {
            var invoices = new List<Invoice>();
            int retryCount = 3;

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        string query = "SELECT * FROM Invoices";
                        var command = new SqlCommand(query, connection);

                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                invoices.Add(new Invoice
                                {
                                    Id = reader["Id"].ToString(),
                                    BillTo = reader["BillTo"].ToString(),
                                    Description = reader["Description"] as string,
                                    DateCreated = reader["DateCreated"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["DateCreated"])
                                        : DateTime.MinValue,
                                    DueDate = reader["DueDate"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["DueDate"])
                                        : DateTime.MinValue,
                                    Status = reader["Status"]?.ToString() ?? "estimate"
                                });
                            }
                        }
                    }

                    return invoices;
                }
                catch (SqlException ex) when (ex.Number == -2) // Handle timeout
                {
                    if (i == retryCount - 1)
                    {
                        throw new Exception("Failed to load invoices after multiple attempts.", ex);
                    }

                    // Wait before retrying (e.g., 2 seconds)
                    System.Threading.Thread.Sleep(2000);
                }
            }

            return invoices;
        }


        public List<InvoiceItem> GetInvoiceItems(string invoiceId)
        {
            var items = new List<InvoiceItem>();
            int retryCount = 3;

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        string query = "SELECT * FROM InvoiceItems WHERE InvoiceId = @InvoiceId";
                        var command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@InvoiceId", invoiceId);

                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new InvoiceItem
                                {
                                    ItemId = reader["ItemId"].ToString(),
                                    InvoiceId = reader["InvoiceId"].ToString(),
                                    Type = reader["Type"].ToString(),
                                    PartId = reader["PartId"] as int?,
                                    Quantity = Convert.ToInt32(reader["Quantity"]),
                                    Rate = Convert.ToDecimal(reader["Rate"]),
                                    Description = reader["Description"].ToString(),
                                    Total = Convert.ToDecimal(reader["Total"]),
                                    OnInvoice = Convert.ToBoolean(reader["OnInvoice"])
                                });
                            }
                        }
                    }

                    return items;
                }
                catch (SqlException ex) when (ex.Number == -2) // Handle timeout
                {
                    if (i == retryCount - 1)
                    {
                        throw new Exception("Failed to load invoice items after multiple attempts.", ex);
                    }

                    // Wait before retrying
                    System.Threading.Thread.Sleep(2000);
                }
            }

            return items;
        }

        public void DeleteInvoiceItem(string itemId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM InvoiceItems WHERE ItemId = @ItemId";
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ItemId", itemId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);

            }
        }

        public void AddInvoiceWithItems(Invoice invoice, List<InvoiceItem> items)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Insert the Invoice
                            string insertInvoiceQuery = @"
                        INSERT INTO Invoices (Id, BillTo, Description, DateCreated, DueDate)
                        VALUES (@Id, @BillTo, @Description, @DateCreated, @DueDate)";

                            var invoiceCommand = new SqlCommand(insertInvoiceQuery, connection, transaction);
                            invoiceCommand.Parameters.AddWithValue("@Id", invoice.Id); // Explicitly set the ID
                            invoiceCommand.Parameters.AddWithValue("@BillTo", invoice.BillTo);
                            invoiceCommand.Parameters.AddWithValue("@Description", invoice.Description ?? (object)DBNull.Value);
                            invoiceCommand.Parameters.AddWithValue("@DateCreated", invoice.DateCreated != default
                                ? invoice.DateCreated : DateTime.Now); // Use current date if not set
                            invoiceCommand.Parameters.AddWithValue("@DueDate", invoice.DueDate != default
                                ? invoice.DueDate : DateTime.Now.AddDays(30)); // Default due date 30 days from now
                            invoiceCommand.Parameters.AddWithValue("@Status", invoice.Status ?? "estimate");

                            invoiceCommand.ExecuteNonQuery();

                            // Insert each InvoiceItem
                            string insertItemQuery = @"
                        INSERT INTO InvoiceItems (ItemId, InvoiceId, Type, PartId, Quantity, Rate, Description)
                        VALUES (@ItemId, @InvoiceId, @Type, @PartId, @Quantity, @Rate, @Description)";

                            foreach (var item in items)
                            {
                                var itemCommand = new SqlCommand(insertItemQuery, connection, transaction);
                                itemCommand.Parameters.AddWithValue("@ItemId", Guid.NewGuid().ToString()); // Generate unique ItemId
                                itemCommand.Parameters.AddWithValue("@InvoiceId", invoice.Id); // Match the InvoiceId
                                itemCommand.Parameters.AddWithValue("@Type", item.Type ?? (object)DBNull.Value);
                                itemCommand.Parameters.AddWithValue("@PartId", item.PartId.HasValue ? (object)item.PartId.Value : DBNull.Value);
                                itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                itemCommand.Parameters.AddWithValue("@Rate", item.Rate);
                                itemCommand.Parameters.AddWithValue("@Description", item.Description ?? (object)DBNull.Value);

                                itemCommand.ExecuteNonQuery();
                            }

                            // Commit the transaction
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            // Rollback the transaction in case of an error
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);
            }
        }


        public void DeleteInvoice(string invoiceId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Delete all associated invoice items
                            string deleteItemsQuery = "DELETE FROM InvoiceItems WHERE InvoiceId = @InvoiceId";
                            var deleteItemsCommand = new SqlCommand(deleteItemsQuery, connection, transaction);
                            deleteItemsCommand.Parameters.AddWithValue("@InvoiceId", invoiceId);
                            deleteItemsCommand.ExecuteNonQuery();

                            // Delete the invoice
                            string deleteInvoiceQuery = "DELETE FROM Invoices WHERE Id = @InvoiceId";
                            var deleteInvoiceCommand = new SqlCommand(deleteInvoiceQuery, connection, transaction);
                            deleteInvoiceCommand.Parameters.AddWithValue("@InvoiceId", invoiceId);
                            deleteInvoiceCommand.ExecuteNonQuery();

                            // Commit the transaction
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            // Rollback the transaction if any error occurs
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting the invoice: {ex.Message}", ex);
            }
        }

        public void UpdateInvoiceItemOnInvoiceStatus(string itemId, bool onInvoice)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE InvoiceItems SET OnInvoice = @OnInvoice WHERE ItemId = @ItemId";
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@OnInvoice", onInvoice);
                    command.Parameters.AddWithValue("@ItemId", itemId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to update OnInvoice status for Item ID: {itemId}. Error: {ex.Message}", ex);
            }
        }


        public void UpdateInvoiceStatus(string invoiceId, string status)
        {
            if (IsInvoiceFinalized(invoiceId))
            {
                throw new InvalidOperationException("Cannot edit a finalized invoice.");
            }
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Invoices SET Status = @Status WHERE Id = @InvoiceId";
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@InvoiceId", invoiceId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("The database operation timed out. Please try again later.", ex);
            }
        }
        public void UpdateInvoiceDescription(string invoiceId, string newDescription)
        {
            if (IsInvoiceFinalized(invoiceId))
            {
                throw new InvalidOperationException("Cannot edit a finalized invoice.");
            }
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Invoices SET Description = @Description WHERE Id = @InvoiceId";
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Description", newDescription ?? (object)DBNull.Value); // Handle null values
                    command.Parameters.AddWithValue("@InvoiceId", invoiceId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to update invoice description. Error: {ex.Message}", ex);
            }
        }

        public void UpdateInvoiceCustomer(string invoiceId, string newCustomerId)
        {
            if (IsInvoiceFinalized(invoiceId))
            {
                throw new InvalidOperationException("Cannot edit a finalized invoice.");
            }
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Invoices SET BillTo = @BillTo WHERE Id = @InvoiceId";
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@BillTo", newCustomerId);
                    command.Parameters.AddWithValue("@InvoiceId", invoiceId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to update invoice customer. Error: {ex.Message}", ex);
            }
        }
        private bool IsInvoiceFinalized(string invoiceId)
        {
            if (string.IsNullOrEmpty(invoiceId))
            {
                throw new ArgumentException("Invoice ID cannot be null or empty.", nameof(invoiceId));
            }

            using (var connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Status FROM Invoices WHERE Id = @InvoiceId";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@InvoiceId", invoiceId);

                connection.Open();
                var statusObj = command.ExecuteScalar();

                if (statusObj == null)
                {
                    // No record found for the given Invoice ID
                    throw new InvalidOperationException($"No invoice found with ID: {invoiceId}");
                }

                string status = statusObj.ToString();
                return status.Equals("finalized", StringComparison.OrdinalIgnoreCase);
            }
        }


        public void AddInvoiceItem(InvoiceItem item, string invoiceId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = @"
                INSERT INTO InvoiceItems (ItemId, InvoiceId, Type, PartId, Quantity, Rate, Description, OnInvoice) 
                VALUES (@ItemId, @InvoiceId, @Type, @PartId, @Quantity, @Rate, @Description, @OnInvoice)";

                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ItemId", Guid.NewGuid().ToString());
                    command.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    command.Parameters.AddWithValue("@Type", item.Type ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PartId", item.PartId.HasValue ? (object)item.PartId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@Quantity", item.Quantity);
                    command.Parameters.AddWithValue("@Rate", item.Rate);
                    command.Parameters.AddWithValue("@Description", item.Description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@OnInvoice", item.OnInvoice);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Failed to add new item. Error: " + ex.Message, ex);
            }
        }

        public void UpdateInvoiceItem(InvoiceItem item)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = @"
                UPDATE InvoiceItems 
                SET Type = @Type, 
                    PartId = @PartId, 
                    Quantity = @Quantity, 
                    Rate = @Rate, 
                    Description = @Description, 
                    OnInvoice = @OnInvoice 
                WHERE ItemId = @ItemId";

                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Type", item.Type ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PartId", item.PartId.HasValue ? (object)item.PartId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@Quantity", item.Quantity);
                    command.Parameters.AddWithValue("@Rate", item.Rate);
                    command.Parameters.AddWithValue("@Description", item.Description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@OnInvoice", item.OnInvoice);
                    command.Parameters.AddWithValue("@ItemId", item.ItemId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to update item with ID {item.ItemId}. Error: {ex.Message}", ex);
            }
        }



    }
}
