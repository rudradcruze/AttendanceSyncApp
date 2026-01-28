using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Helpers
{
    /// <summary>
    /// Helper class for dynamic database connections to external company databases.
    /// Enables attendance synchronization with multiple external SQL Server databases
    /// by creating contexts on-the-fly with decrypted credentials.
    /// </summary>
    public static class DynamicDbHelper
    {
        /// <summary>
        /// Builds a SQL Server connection string from DatabaseConfiguration.
        /// Decrypts the stored password before building the connection string.
        /// </summary>
        /// <param name="config">The database configuration containing connection details.</param>
        /// <returns>A complete SQL Server connection string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if config is null.</exception>
        public static string BuildConnectionString(DatabaseConfiguration config)
        {
            // Validate configuration is provided
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Decrypt the stored password using encryption helper
            var decryptedPassword = EncryptionHelper.Decrypt(config.DatabasePassword);

            // Build SQL Server connection string with all necessary parameters
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = config.DatabaseIP,               // Server IP/hostname
                InitialCatalog = config.DatabaseName,         // Database name
                UserID = config.DatabaseUserId,               // SQL Server username
                Password = decryptedPassword,                 // Decrypted password
                MultipleActiveResultSets = true,              // Enable MARS for complex queries
                ConnectTimeout = 30                           // 30 second connection timeout
            };

            return builder.ConnectionString;
        }

        /// <summary>
        /// Creates an AppDbContext with a dynamic connection string for the external database.
        /// This allows querying external company databases using Entity Framework.
        /// </summary>
        /// <param name="config">The database configuration.</param>
        /// <returns>A new AppDbContext connected to the external database.</returns>
        public static AppDbContext CreateExternalDbContext(DatabaseConfiguration config)
        {
            // Build connection string and create context
            var connectionString = BuildConnectionString(config);
            return new AppDbContext(connectionString);
        }

        /// <summary>
        /// Creates an AttandanceSynchronization record in the external database.
        /// Uses the first CompanyId from the external database's Company table.
        /// </summary>
        /// <param name="config">The database configuration.</param>
        /// <param name="fromDate">Start date for synchronization period.</param>
        /// <param name="toDate">End date for synchronization period.</param>
        /// <param name="companyId">Company ID (not used, uses first company from external DB).</param>
        /// <returns>The ID of the created sync record, or null if failed.</returns>
        /// <exception cref="Exception">Thrown if connection fails or no company found.</exception>
        public static int? CreateSyncInExternalDb(DatabaseConfiguration config, DateTime fromDate, DateTime toDate, int companyId)
        {
            try
            {
                using (var context = CreateExternalDbContext(config))
                {
                    // Query the external database Company table and get the first CompanyId
                    // This is necessary because each external database may have different company IDs
                    var firstCompany = context.Companies.OrderBy(c => c.Id).FirstOrDefault();

                    // Ensure at least one company exists in the external database
                    if (firstCompany == null)
                    {
                        throw new Exception("No company found in the external database");
                    }

                    // Create new sync request in the external database
                    var sync = new AttandanceSynchronization
                    {
                        FromDate = fromDate,
                        ToDate = toDate,
                        CompanyId = firstCompany.Id, // Use the first company ID from external database
                        Status = "NR" // New Request - initial status
                    };

                    // Add and save the sync record
                    context.AttandanceSynchronizations.Add(sync);
                    context.SaveChanges();

                    // Return the auto-generated ID of the inserted record
                    return sync.Id;
                }
            }
            catch (Exception ex)
            {
                // Re-throw with more context for debugging
                // The calling service will catch this and return to client
                throw new Exception($"External DB Connection/Creation Failed: {ex.Message} (DB: {config.DatabaseName})", ex);
            }
        }

        /// <summary>
        /// Tests the connection to an external database to verify credentials and connectivity.
        /// </summary>
        /// <param name="config">The database configuration to test.</param>
        /// <returns>True if connection successful, false if any error occurs.</returns>
        public static bool TestConnection(DatabaseConfiguration config)
        {
            try
            {
                // Build connection string and attempt to open connection
                var connectionString = BuildConnectionString(config);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Connection successful
                    return true;
                }
            }
            catch
            {
                // Any connection error returns false
                return false;
            }
        }

        /// <summary>
        /// Gets the current status of a sync record from the external database.
        /// Used to check if external database has completed processing.
        /// </summary>
        /// <param name="config">The database configuration.</param>
        /// <param name="externalSyncId">The ID of the sync record in the external database.</param>
        /// <returns>Status code (NR, IP, CP, etc.) or "Unknown"/"Error".</returns>
        public static string GetSyncStatusFromExternalDb(DatabaseConfiguration config, int externalSyncId)
        {
            try
            {
                using (var context = CreateExternalDbContext(config))
                {
                    // Find the sync record by ID
                    var sync = context.AttandanceSynchronizations.Find(externalSyncId);
                    // Return status or "Unknown" if record not found
                    return sync?.Status ?? "Unknown";
                }
            }
            catch
            {
                // Return "Error" if any exception occurs
                return "Error";
            }
        }
    }
}
