using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models.ConcurrentSimulation;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.ConcurrentSimulation;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.ConcurrentSimulation;

namespace AttandanceSyncApp.Services.ConcurrentSimulation
{
    /// <summary>
    /// Service responsible for executing concurrent simulation operations.
    /// Handles server/database discovery, period-end data extraction,
    /// and concurrent insertion of period-end processing requests.
    /// </summary>
    public class ConcurrentSimulationService : IConcurrentSimulationService
    {
        /// Unit of work for accessing repositories.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of ConcurrentSimulationService.
        /// </summary>
        /// <param name="unitOfWork">Authentication unit of work.</param>
        public ConcurrentSimulationService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Calculates the first day of the previous month.
        /// </summary>
        /// <returns>Previous month start date in yyyy-MM-dd format.</returns>
        private string GetLastMonthDate()
        {
            var now = DateTime.Now;
            var lastMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
            return lastMonth.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Retrieves all active server IP addresses.
        /// </summary>
        /// <returns>List of active server IPs.</returns>
        public ServiceResult<IEnumerable<ServerIpDto>> GetAllServerIps()
        {
            try
            {
                // Fetch active servers and map to DTOs
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

                return ServiceResult<IEnumerable<ServerIpDto>>
                    .SuccessResult(serverIps);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ServerIpDto>>
                    .FailureResult($"Error fetching server IPs: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves accessible databases for a given server IP.
        /// </summary>
        /// <param name="serverIpId">Server IP identifier.</param>
        /// <returns>List of accessible databases.</returns>
        public ServiceResult<IEnumerable<DatabaseListDto>> GetDatabasesForServer(int serverIpId)
        {
            try
            {
                // Validate server IP existence
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<DatabaseListDto>>
                        .FailureResult("Server IP not found");
                }

                // Retrieve database access permissions
                var accessibleDatabases = new HashSet<string>(
                    _unitOfWork.DatabaseAccess
                        .GetAccessibleDatabasesByServerId(serverIpId)
                        .Select(da => da.DatabaseName)
                );

                // If no databases are accessible, return empty list
                if (!accessibleDatabases.Any())
                {
                    return ServiceResult<IEnumerable<DatabaseListDto>>
                        .SuccessResult(new List<DatabaseListDto>());
                }

                // Decrypt database password and build connection string
                var decryptedPassword =
                    EncryptionHelper.Decrypt(serverIp.DatabasePassword);

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

                                // Include only databases the server has access to
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
        /// Retrieves period-end processing data for the previous month.
        /// </summary>
        /// <param name="serverIpId">Server IP identifier.</param>
        /// <param name="databaseName">Target database name.</param>
        /// <returns>Collection of period-end processing entries.</returns>
        public ServiceResult<IEnumerable<PeriodEndProcessEntry>> GetPeriodEndData(
            int serverIpId,
            string databaseName)
        {
            try
            {
                // Validate server IP
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<PeriodEndProcessEntry>>
                        .FailureResult("Server IP not found");
                }

                // Decrypt password and build connection string
                var decryptedPassword =
                    EncryptionHelper.Decrypt(serverIp.DatabasePassword);

                var connectionString = BuildConnectionString(
                    serverIp.IpAddress,
                    serverIp.DatabaseUser,
                    decryptedPassword,
                    databaseName
                );

                var entries = new List<PeriodEndProcessEntry>();

                // SQL query to generate period-end request data
                var query = @"
                    SELECT
                        ISNULL(MIN(u.Id), '') AS UserId,
                        ISNULL(l.BranchID, 0) AS Branch_Id,
                        ISNULL(p.Location_Id, 0) AS Location_Id,
                        1                     AS CompanyId,
                        'NR'                  AS Status,
                        0                     AS EmployeeId,
                        'NA'                  AS PostProcessStatus
                    FROM far.tblperiodenddetails p
                    INNER JOIN far.tblLocation l
                            ON l.Id = p.Location_Id
                    INNER JOIN Users u
                            ON u.BranchId = l.BranchID
                    WHERE p.PeriodFrom = @PeriodFrom
                    GROUP BY
                        l.BranchID,
                        p.Location_Id";

                // Execute query and map results
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@PeriodFrom",
                            GetLastMonthDate());

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                entries.Add(new PeriodEndProcessEntry
                                {
                                    UserId = reader.GetString(0),
                                    Branch_Id = reader.GetInt32(1),
                                    Location_Id = reader.GetInt32(2),
                                    CompanyId = reader.GetInt32(3),
                                    Status = reader.GetString(4),
                                    EmployeeId = reader.GetInt32(5),
                                    PostProcessStatus = reader.GetString(6)
                                });
                            }
                        }
                    }
                }

                return ServiceResult<IEnumerable<PeriodEndProcessEntry>>
                    .SuccessResult(entries);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<PeriodEndProcessEntry>>
                    .FailureResult($"Error fetching period end data: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes concurrent insertion of period-end processing requests.
        /// </summary>
        /// <param name="request">Concurrent simulation request data.</param>
        /// <returns>Execution summary including success and failure counts.</returns>
        public ServiceResult<HitConcurrentResponseDto> HitConcurrent(
            HitConcurrentRequestDto request)
        {
            try
            {
                // Fetch period-end data
                var dataResult =
                    GetPeriodEndData(request.ServerIpId, request.DatabaseName);

                if (!dataResult.Success)
                {
                    return ServiceResult<HitConcurrentResponseDto>
                        .FailureResult($"Failed to fetch data: {dataResult.Message}");
                }

                var entries = dataResult.Data.ToList();
                if (!entries.Any())
                {
                    return ServiceResult<HitConcurrentResponseDto>
                        .FailureResult("No entries to insert");
                }

                // Validate server IP
                var serverIp = _unitOfWork.ServerIps.GetById(request.ServerIpId);
                if (serverIp == null)
                {
                    return ServiceResult<HitConcurrentResponseDto>
                        .FailureResult("Server IP not found");
                }

                // Build database connection string
                var decryptedPassword =
                    EncryptionHelper.Decrypt(serverIp.DatabasePassword);

                var connectionString = BuildConnectionString(
                    serverIp.IpAddress,
                    serverIp.DatabaseUser,
                    decryptedPassword,
                    request.DatabaseName
                );

                // Thread-safe collections for concurrent execution
                var errors = new ConcurrentBag<string>();
                var successCount = 0;

                var insertQuery = @"
                    INSERT INTO far.PeriodEndProcessRequest
                    (CompanyId, Status, EmployeeId, UserId, Branch_Id, Location_Id, PostProcessStatus)
                    VALUES
                    (@CompanyId, @Status, @EmployeeId, @UserId, @Branch_Id, @Location_Id, @PostProcessStatus)";

                // Execute insert operations concurrently
                var tasks = entries.Select(entry => Task.Run(() =>
                {
                    try
                    {
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand(insertQuery, connection))
                            {
                                command.Parameters.AddWithValue("@CompanyId", entry.CompanyId);
                                command.Parameters.AddWithValue("@Status", entry.Status ?? "NR");
                                command.Parameters.AddWithValue("@EmployeeId", entry.EmployeeId);
                                command.Parameters.AddWithValue("@UserId", entry.UserId ?? "");
                                command.Parameters.AddWithValue("@Branch_Id", entry.Branch_Id);
                                command.Parameters.AddWithValue("@Location_Id", entry.Location_Id);
                                command.Parameters.AddWithValue("@PostProcessStatus", entry.PostProcessStatus ?? "NA");

                                command.ExecuteNonQuery();
                                System.Threading.Interlocked
                                    .Increment(ref successCount);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var inner =
                            ex.InnerException != null
                                ? $" Inner: {ex.InnerException.Message}"
                                : "";

                        errors.Add(
                            $"Error inserting entry (Location_Id: {entry.Location_Id}, UserId: {entry.UserId}): {ex.Message}{inner}");
                    }
                })).ToArray();

                Task.WaitAll(tasks);

                var response = new HitConcurrentResponseDto
                {
                    TotalRecords = entries.Count,
                    SuccessCount = successCount,
                    FailedCount = errors.Count,
                    Errors = errors.ToList()
                };

                return errors.Any()
                    ? ServiceResult<HitConcurrentResponseDto>
                        .SuccessResult(response, $"Completed with {errors.Count} errors")
                    : ServiceResult<HitConcurrentResponseDto>
                        .SuccessResult(response, $"Successfully inserted {successCount} records simultaneously");
            }
            catch (Exception ex)
            {
                return ServiceResult<HitConcurrentResponseDto>
                    .FailureResult($"Error during concurrent insert: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds a SQL Server connection string using explicit credentials.
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
