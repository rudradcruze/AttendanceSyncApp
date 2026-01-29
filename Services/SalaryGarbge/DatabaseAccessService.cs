using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;
using AttandanceSyncApp.Models.SalaryGarbge;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Services.SalaryGarbge
{
    /// <summary>
    /// Service responsible for managing database access permissions
    /// for a given server IP. Handles discovery of databases,
    /// access assignment, updates, and soft deletion.
    /// </summary>
    public class DatabaseAccessService : IDatabaseAccessService
    {
        /// Unit of work for accessing repositories.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of DatabaseAccessService.
        /// </summary>
        /// <param name="unitOfWork">Authentication unit of work.</param>
        public DatabaseAccessService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all databases from the server and merges them
        /// with existing access records to show access status.
        /// </summary>
        /// <param name="serverIpId">Server IP identifier.</param>
        /// <returns>List of databases with access metadata.</returns>
        public ServiceResult<IEnumerable<DatabaseAccessListDto>>
            GetDatabasesWithAccessStatus(int serverIpId)
        {
            try
            {
                // Validate server IP existence
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<DatabaseAccessListDto>>
                        .FailureResult("Server IP not found");
                }

                // Retrieve databases directly from SQL Server
                var actualDatabases = GetDatabasesFromServer(serverIp);

                // Retrieve existing access records for the server
                var existingAccess = _unitOfWork.DatabaseAccess
                    .GetByServerIpId(serverIpId)
                    .ToDictionary(
                        da => da.DatabaseName,
                        da => da
                    );

                // Combine actual databases with access table data
                var result = actualDatabases.Select(dbName =>
                    new DatabaseAccessListDto
                    {
                        DatabaseName = dbName,

                        // Indicates if database exists in access table
                        ExistsInAccessTable =
                            existingAccess.ContainsKey(dbName),

                        // Access flag from table or default false
                        HasAccess =
                            existingAccess.ContainsKey(dbName)
                                ? existingAccess[dbName].HasAccess
                                : false,

                        // DatabaseAccess table record ID if exists
                        DatabaseAccessId =
                            existingAccess.ContainsKey(dbName)
                                ? existingAccess[dbName].Id
                                : (int?)null
                    })
                    .OrderBy(d => d.DatabaseName)
                    .ToList();

                return ServiceResult<IEnumerable<DatabaseAccessListDto>>
                    .SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<DatabaseAccessListDto>>
                    .FailureResult($"Failed to retrieve databases: {ex.Message}");
            }
        }

        /// <summary>
        /// Grants database access for a specific server and database.
        /// </summary>
        /// <param name="serverIpId">Server IP identifier.</param>
        /// <param name="databaseName">Database name.</param>
        /// <returns>Operation result.</returns>
        public ServiceResult AddDatabaseAccess(
            int serverIpId,
            string databaseName)
        {
            try
            {
                // Validate database name
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    return ServiceResult
                        .FailureResult("Database name is required");
                }

                // Validate server IP
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult
                        .FailureResult("Server IP not found");
                }

                // Prevent duplicate access records
                if (_unitOfWork.DatabaseAccess
                    .DatabaseAccessExists(serverIpId, databaseName))
                {
                    return ServiceResult
                        .FailureResult("Database access already exists");
                }

                // Create new database access record
                var dbAccess = new DatabaseAccess
                {
                    ServerIpId = serverIpId,
                    DatabaseName = databaseName.Trim(),
                    HasAccess = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.DatabaseAccess.Add(dbAccess);
                _unitOfWork.SaveChanges();

                return ServiceResult
                    .SuccessResult("Database access added successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to add database access: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates access permission for an existing database.
        /// </summary>
        /// <param name="serverIpId">Server IP identifier.</param>
        /// <param name="databaseName">Database name.</param>
        /// <param name="hasAccess">Access flag.</param>
        /// <returns>Operation result.</returns>
        public ServiceResult UpdateDatabaseAccess(
            int serverIpId,
            string databaseName,
            bool hasAccess)
        {
            try
            {
                // Retrieve existing access record
                var dbAccess = _unitOfWork.DatabaseAccess
                    .GetByServerIpAndDatabase(serverIpId, databaseName);

                if (dbAccess == null)
                {
                    return ServiceResult
                        .FailureResult("Database access not found");
                }

                // Update access state
                dbAccess.HasAccess = hasAccess;
                dbAccess.UpdatedAt = DateTime.Now;

                _unitOfWork.DatabaseAccess.Update(dbAccess);
                _unitOfWork.SaveChanges();

                var status = hasAccess ? "granted" : "revoked";
                return ServiceResult
                    .SuccessResult($"Database access {status} successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to update database access: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes database access using soft delete.
        /// </summary>
        /// <param name="serverIpId">Server IP identifier.</param>
        /// <param name="databaseName">Database name.</param>
        /// <returns>Operation result.</returns>
        public ServiceResult RemoveDatabaseAccess(
            int serverIpId,
            string databaseName)
        {
            try
            {
                // Retrieve access record
                var dbAccess = _unitOfWork.DatabaseAccess
                    .GetByServerIpAndDatabase(serverIpId, databaseName);

                if (dbAccess == null)
                {
                    return ServiceResult
                        .FailureResult("Database access not found");
                }

                // Soft delete the record
                dbAccess.IsActive = false;
                dbAccess.UpdatedAt = DateTime.Now;

                _unitOfWork.DatabaseAccess.Update(dbAccess);
                _unitOfWork.SaveChanges();

                return ServiceResult
                    .SuccessResult("Database access removed successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to remove database access: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all user databases from the SQL Server instance.
        /// </summary>
        /// <param name="serverIp">Server IP entity.</param>
        /// <returns>List of database names.</returns>
        private List<string> GetDatabasesFromServer(ServerIp serverIp)
        {
            var databases = new List<string>();

            // Decrypt stored database password
            var decryptedPassword =
                EncryptionHelper.Decrypt(serverIp.DatabasePassword);

            // Build connection string for master database
            var connectionString = BuildConnectionString(
                serverIp.IpAddress,
                serverIp.DatabaseUser,
                decryptedPassword);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Query only online, non-system databases
                var query = @"
                    SELECT name FROM sys.databases
                    WHERE state_desc = 'ONLINE'
                      AND name NOT IN ('master', 'tempdb', 'model', 'msdb')
                    ORDER BY name";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        databases.Add(reader.GetString(0));
                    }
                }
            }

            return databases;
        }

        /// <summary>
        /// Builds a SQL Server connection string using explicit credentials.
        /// </summary>
        private string BuildConnectionString(
            string serverIp,
            string userId,
            string password)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = serverIp,
                InitialCatalog = "master",
                UserID = userId,
                Password = password,
                IntegratedSecurity = false,
                ConnectTimeout = 30,
                Encrypt = false,
                TrustServerCertificate = true
            };

            return builder.ConnectionString;
        }
    }
}
