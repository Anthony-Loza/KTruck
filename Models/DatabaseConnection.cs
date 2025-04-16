using System;
using Microsoft.Data.Sqlite;

namespace KTruckGui.Data
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection()
        {
            // Set the connection string to your SQLite database file
            // Adjust the path as needed for your project
            _connectionString = "Data Source=KTruck.db";
        }

        public SqliteConnection GetConnection()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}