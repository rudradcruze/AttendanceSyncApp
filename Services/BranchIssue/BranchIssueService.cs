using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.BranchIssue;
using AttandanceSyncApp.Models.DTOs.ConcurrentSimulation;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Repositories.Interfaces.BranchIssue;
using AttandanceSyncApp.Services.Interfaces.BranchIssue;

namespace AttandanceSyncApp.Services.BranchIssue
{
    /// <summary>
    /// Service responsible for handling branch-related issues such as
    /// server/database discovery, problem branch identification,
    /// and branch reprocessing operations.
    /// </summary>
    public class BranchIssueService : IBranchIssueService
    {
        /// Unit of work for accessing shared repositories.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// Repository for executing branch issue–specific database operations.
        private readonly IBranchIssueRepository _repository;

        /// <summary>
        /// Initializes a new instance of BranchIssueService.
        /// </summary>
        /// <param name="unitOfWork">Authentication unit of work.</param>
        /// <param name="repository">Branch issue repository.</param>
        public BranchIssueService(IAuthUnitOfWork unitOfWork, IBranchIssueRepository repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        /// <summary>
        /// Retrieves all active server IP addresses.
        /// </summary>
        /// <returns>List of active server IPs with descriptions.</returns>
        public ServiceResult<IEnumerable<ServerIpDto>> GetAllServerIps()
        {
            try
            {
                // Fetch active server IPs and map to DTOs
                var serverIps = _unitOfWork.ServerIps.GetAll()
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.IpAddress)
                    .Select(s => new ServerIpDto
                    {
                        Id = s.Id,
                        IpAddress = s.IpAddress,
                        Description = s.Description
                    })
                    .ToList();

                return ServiceResult<IEnumerable<ServerIpDto>>.SuccessResult(serverIps);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ServerIpDto>>
                    .FailureResult($"Error fetching server IPs: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves accessible databases for a specific server IP.
        /// </summary>
        /// <param name="serverIpId">Server IP identifier.</param>
        /// <returns>List of accessible databases.</returns>
        public ServiceResult<IEnumerable<DatabaseListDto>> GetDatabasesForServer(int serverIpId)
        {
            try
            {
                // Validate server IP
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<DatabaseListDto>>
                        .FailureResult("Server IP not found");
                }

                // Get databases the server is allowed to access
                var accessibleDatabases = new HashSet<string>(
                    _unitOfWork.DatabaseAccess
                        .GetAccessibleDatabasesByServerId(serverIpId)
                        .Select(da => da.DatabaseName)
                );

                // If no accessible databases exist, return empty list
                if (!accessibleDatabases.Any())
                {
                    return ServiceResult<IEnumerable<DatabaseListDto>>
                        .SuccessResult(new List<DatabaseListDto>());
                }

                // Decrypt password and build SQL connection string
                var decryptedPassword = EncryptionHelper.Decrypt(serverIp.DatabasePassword);
                var connectionString = BuildConnectionString(
                    serverIp.IpAddress,
                    serverIp.DatabaseUser,
                    decryptedPassword,
                    "master"
                );

                var databases = new List<DatabaseListDto>();

                // Connect to SQL Server and retrieve database list
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(
                        "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name",
                        connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var dbName = reader.GetString(0);
                                if (accessibleDatabases.Contains(dbName))
                                {
                                    databases.Add(new DatabaseListDto
                                    {
                                        DatabaseName = dbName
                                    });
                                }
                            }
                        }
                    }
                }

                return ServiceResult<IEnumerable<DatabaseListDto>>
                    .SuccessResult(databases);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<DatabaseListDto>>
                    .FailureResult($"Error fetching databases: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the start date of the previous month from the database.
        /// </summary>
        /// <param name="serverIpId">Server IP identifier.</param>
        /// <param name="databaseName">Database name.</param>
        /// <returns>Last month start date in yyyy-MM-dd format.</returns>
        public ServiceResult<string> GetLastMonthDate(int serverIpId, string databaseName)
        {
            try
            {
                // Build connection string
                var connectionString = GetConnectionString(serverIpId, databaseName);
                if (string.IsNullOrEmpty(connectionString))
                    return ServiceResult<string>
                        .FailureResult("Could not build connection string");

                // Fetch date from repository
                var date = _repository.GetLastMonthDate(connectionString);
                return ServiceResult<string>
                    .SuccessResult(date.ToString("yyyy-MM-dd"));
            }
            catch (Exception ex)
            {
                return ServiceResult<string>
                    .FailureResult($"Error getting last month date: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves branches with processing issues for a given period.
        /// </summary>
        /// <param name="serverIpId">Server IP identifier.</param>
        /// <param name="databaseName">Database name.</param>
        /// <param name="monthStartDate">Start date of the month.</param>
        /// <param name="locationId">Optional location filter.</param>
        /// <returns>List of problem branches.</returns>
        public ServiceResult<IEnumerable<ProblemBranchDto>> GetProblemBranches(
            int serverIpId,
            string databaseName,
            string monthStartDate,
            string locationId)
        {
            try
            {
                // Build connection string
                var connectionString = GetConnectionString(serverIpId, databaseName);
                if (string.IsNullOrEmpty(connectionString))
                    return ServiceResult<IEnumerable<ProblemBranchDto>>
                        .FailureResult("Could not build connection string");

                // Validate date format
                if (!DateTime.TryParse(monthStartDate, out DateTime parsedDate))
                {
                    return ServiceResult<IEnumerable<ProblemBranchDto>>
                        .FailureResult("Invalid date format");
                }

                // Fetch problematic branches
                var branches = _repository
                    .GetProblemBranches(connectionString, parsedDate, locationId);

                // Map entities to DTOs
                var dtos = branches.Select(b => new ProblemBranchDto
                {
                    PeriodFrom = b.PeriodFrom,
                    BranchCode = b.BranchCode,
                    BranchName = b.BranchName,
                    Remarks = b.Remarks
                }).ToList();

                return ServiceResult<IEnumerable<ProblemBranchDto>>
                    .SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ProblemBranchDto>>
                    .FailureResult($"Error fetching problem branches: {ex.Message}");
            }
        }

        /// <summary>
        /// Reprocesses a specific branch for the given period.
        /// </summary>
        /// <param name="request">Reprocessing request data.</param>
        /// <returns>Operation result message.</returns>
        public ServiceResult<string> ReprocessBranch(ReprocessBranchRequestDto request)
        {
            try
            {
                // Build connection string
                var connectionString = GetConnectionString(
                    request.ServerIpId,
                    request.DatabaseName
                );

                if (string.IsNullOrEmpty(connectionString))
                    return ServiceResult<string>
                        .FailureResult("Could not build connection string");

                // Execute reprocessing
                _repository.ReprocessBranch(
                    connectionString,
                    request.BranchCode,
                    request.Month,
                    request.PrevMonth
                );

                return ServiceResult<string>
                    .SuccessResult("Branch reprocessed successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>
                    .FailureResult($"Error reprocessing branch: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds a database connection string for a server and database.
        /// </summary>
        private string GetConnectionString(int serverIpId, string databaseName)
        {
            var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
            if (serverIp == null) return null;

            var decryptedPassword =
                EncryptionHelper.Decrypt(serverIp.DatabasePassword);

            return BuildConnectionString(
                serverIp.IpAddress,
                serverIp.DatabaseUser,
                decryptedPassword,
                databaseName
            );
        }

        /// <summary>
        /// Creates a SQL Server connection string using explicit credentials.
        /// </summary>
        private string BuildConnectionString(
            string ipAddress,
            string userId,
            string password,
            string databaseName)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = ipAddress,
                InitialCatalog = databaseName,
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
