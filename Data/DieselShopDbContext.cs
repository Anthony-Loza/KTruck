using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
namespace KTruckGui.Data
{
    public class DieselShopDbContext : DbContext
    {
        private const string EnvironmentVariableName = "DieselShopDb";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Retrieve the connection string from the environment variable
            var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariableName);

            // Throw an exception if the connection string is not set
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception($"Connection string not found. Ensure the environment variable '{EnvironmentVariableName}' is set.");
            }

            // Use the connection string to configure the SQL Server database
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}