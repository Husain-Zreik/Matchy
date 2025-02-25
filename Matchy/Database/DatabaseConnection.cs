using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Matchy.Database
{
    public static class DatabaseConnection
    {
        private static readonly string _connectionString;

        static DatabaseConnection()
        {
            var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "1433";
            var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "MemoryGameDB";
            var username = Environment.GetEnvironmentVariable("DATABASE_USER") ?? "sa";
            var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "root";

            _connectionString = $"Server={host},{port};Database={database};User Id={username};Password={password};Encrypt=false;";

            // Debug log for the connection string
            Debug.WriteLine($"Database Connection String: {_connectionString}");
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);  // Use Microsoft.Data.SqlClient.SqlConnection
        }

        public static bool CheckConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    Debug.WriteLine("Attempting to connect to the database...");
                    connection.Open();  // Try to open the connection

                    Debug.WriteLine("Database connection successful.");
                    return true;  // If successful, return true
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"Error connecting to the database: {ex.Message}");
                return false;  // If there is an exception, return false
            }
        }
    }
}
