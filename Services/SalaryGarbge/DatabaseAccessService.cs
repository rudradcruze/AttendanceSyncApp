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
    public class DatabaseAccessService : IDatabaseAccessService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public DatabaseAccessService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<IEnumerable<DatabaseAccessListDto>> GetDatabasesWithAccessStatus(int serverIpId)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<DatabaseAccessListDto>>.FailureResult("Server IP not found");
                }

                // Get databases from actual server
                var actualDatabases = GetDatabasesFromServer(serverIp);

                // Get existing access records
                var existingAccess = _unitOfWork.DatabaseAccess
                    .GetByServerIpId(serverIpId)
                    .ToDictionary(da => da.DatabaseName, da => da);

                // Build combined list
                var result = actualDatabases.Select(dbName => new DatabaseAccessListDto
                {
                    DatabaseName = dbName,
                    ExistsInAccessTable = existingAccess.ContainsKey(dbName),
                    HasAccess = existingAccess.ContainsKey(dbName)
                        ? existingAccess[dbName].HasAccess
                        : false, // Default to false for new databases
                    DatabaseAccessId = existingAccess.ContainsKey(dbName)
                        ? existingAccess[dbName].Id
                        : (int?)null
                }).OrderBy(d => d.DatabaseName).ToList();

                return ServiceResult<IEnumerable<DatabaseAccessListDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<DatabaseAccessListDto>>.FailureResult(
                    $"Failed to retrieve databases: {ex.Message}");
            }
        }

        public ServiceResult AddDatabaseAccess(int serverIpId, string databaseName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    return ServiceResult.FailureResult("Database name is required");
                }

                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult.FailureResult("Server IP not found");
                }

                // Check if already exists
                if (_unitOfWork.DatabaseAccess.DatabaseAccessExists(serverIpId, databaseName))
                {
                    return ServiceResult.FailureResult("Database access already exists");
                }

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

                return ServiceResult.SuccessResult("Database access added successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to add database access: {ex.Message}");
            }
        }

        public ServiceResult UpdateDatabaseAccess(int serverIpId, string databaseName, bool hasAccess)
        {
            try
            {
                var dbAccess = _unitOfWork.DatabaseAccess
                    .GetByServerIpAndDatabase(serverIpId, databaseName);

                if (dbAccess == null)
                {
                    return ServiceResult.FailureResult("Database access not found");
                }

                dbAccess.HasAccess = hasAccess;
                dbAccess.UpdatedAt = DateTime.Now;

                _unitOfWork.DatabaseAccess.Update(dbAccess);
                _unitOfWork.SaveChanges();

                var status = hasAccess ? "granted" : "revoked";
                return ServiceResult.SuccessResult($"Database access {status} successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to update database access: {ex.Message}");
            }
        }

        public ServiceResult RemoveDatabaseAccess(int serverIpId, string databaseName)
        {
            try
            {
                var dbAccess = _unitOfWork.DatabaseAccess
                    .GetByServerIpAndDatabase(serverIpId, databaseName);

                if (dbAccess == null)
                {
                    return ServiceResult.FailureResult("Database access not found");
                }

                // Soft delete
                dbAccess.IsActive = false;
                dbAccess.UpdatedAt = DateTime.Now;

                _unitOfWork.DatabaseAccess.Update(dbAccess);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Database access removed successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to remove database access: {ex.Message}");
            }
        }

        private List<string> GetDatabasesFromServer(ServerIp serverIp)
        {
            var databases = new List<string>();
            var decryptedPassword = EncryptionHelper.Decrypt(serverIp.DatabasePassword);

            var connectionString = BuildConnectionString(
                serverIp.IpAddress,
                serverIp.DatabaseUser,
                decryptedPassword);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT name FROM sys.databases
                    WHERE state_desc = 'ONLINE'
                    AND name NOT IN ('master', 'tempdb', 'model', 'msdb')
                    ORDER BY name";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            databases.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return databases;
        }

        private string BuildConnectionString(string serverIp, string userId, string password)
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
