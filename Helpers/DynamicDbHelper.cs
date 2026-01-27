using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Helpers
{
    /// <summary>
    /// Helper class for dynamic database connections to external company databases
    /// </summary>
    public static class DynamicDbHelper
    {
        /// <summary>
        /// Builds a SQL Server connection string from DatabaseConfiguration
        /// </summary>
        public static string BuildConnectionString(DatabaseConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Decrypt the password
            var decryptedPassword = EncryptionHelper.Decrypt(config.DatabasePassword);

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = config.DatabaseIP,
                InitialCatalog = config.DatabaseName,
                UserID = config.DatabaseUserId,
                Password = decryptedPassword,
                MultipleActiveResultSets = true,
                ConnectTimeout = 30
            };

            return builder.ConnectionString;
        }

        /// <summary>
        /// Creates an AppDbContext with a dynamic connection string for the external database
        /// </summary>
        public static AppDbContext CreateExternalDbContext(DatabaseConfiguration config)
        {
            var connectionString = BuildConnectionString(config);
            return new AppDbContext(connectionString);
        }

        /// <summary>
        /// Creates an AttandanceSynchronization record in the external database
        /// Gets the first CompanyId from the external database's Company table
        /// </summary>
        /// <returns>The ID of the created record, or null if failed</returns>
        public static int? CreateSyncInExternalDb(DatabaseConfiguration config, DateTime fromDate, DateTime toDate, int companyId)
        {
            try
            {
                using (var context = CreateExternalDbContext(config))
                {
                    // Query the external database Company table and get the first CompanyId
                    var firstCompany = context.Companies.OrderBy(c => c.Id).FirstOrDefault();

                    if (firstCompany == null)
                    {
                        throw new Exception("No company found in the external database");
                    }

                    var sync = new AttandanceSynchronization
                    {
                        FromDate = fromDate,
                        ToDate = toDate,
                        CompanyId = firstCompany.Id, // Use the first company ID from external database
                        Status = "NR" // New Request
                    };

                    context.AttandanceSynchronizations.Add(sync);
                    context.SaveChanges();

                    // Return the inserted ID
                    return sync.Id;
                }
            }
            catch (Exception ex)
            {
                // Throwing the exception allows the calling service to catch it and return the specific error message to the client
                throw new Exception($"External DB Connection/Creation Failed: {ex.Message} (DB: {config.DatabaseName})", ex);
            }
        }

        /// <summary>
        /// Tests the connection to an external database
        /// </summary>
        public static bool TestConnection(DatabaseConfiguration config)
        {
            try
            {
                var connectionString = BuildConnectionString(config);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the status of a sync record from the external database
        /// </summary>
        public static string GetSyncStatusFromExternalDb(DatabaseConfiguration config, int externalSyncId)
        {
            try
            {
                using (var context = CreateExternalDbContext(config))
                {
                    var sync = context.AttandanceSynchronizations.Find(externalSyncId);
                    return sync?.Status ?? "Unknown";
                }
            }
            catch
            {
                return "Error";
            }
        }
    }
}
