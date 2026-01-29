using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Sync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Sync;

namespace AttandanceSyncApp.Services.Sync
{
    /// <summary>
    /// Service responsible for handling dynamic database operations.
    /// Supports connection testing and runtime data retrieval
    /// based on company-specific database configurations.
    /// </summary>
    public class DynamicDatabaseService : IDynamicDatabaseService
    {
        /// Unit of work for accessing repositories.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of DynamicDatabaseService.
        /// </summary>
        /// <param name="unitOfWork">Authentication unit of work.</param>
        public DynamicDatabaseService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Tests connectivity to a database using provided connection details.
        /// </summary>
        /// <param name="config">Database connection configuration.</param>
        /// <returns>True if connection succeeds; otherwise false.</returns>
        public ServiceResult<bool> TestConnection(DatabaseConnectionDto config)
        {
            try
            {
                // Build SQL Server connection string
                var connectionString = BuildConnectionString(config);

                // Attempt to open connection
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return ServiceResult<bool>
                        .SuccessResult(true, "Connection successful");
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>
                    .FailureResult($"Connection failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves attendance synchronization data from a dynamically
        /// assigned company database.
        /// </summary>
        /// <param name="requestId">Attendance sync request ID.</param>
        /// <param name="fromDate">Optional start date filter.</param>
        /// <param name="toDate">Optional end date filter.</param>
        /// <returns>Filtered attendance synchronization records.</returns>
        public ServiceResult<IEnumerable<AttandanceSynchronization>>
            GetAttendanceData(
                int requestId,
                DateTime? fromDate,
                DateTime? toDate)
        {
            try
            {
                // Retrieve attendance sync request
                var request =
                    _unitOfWork.AttandanceSyncRequests.GetById(requestId);

                if (request == null)
                {
                    return ServiceResult<IEnumerable<AttandanceSynchronization>>
                        .FailureResult("Request not found");
                }

                // Retrieve database configuration for the company
                var dbConfig =
                    _unitOfWork.DatabaseConfigurations
                        .GetByCompanyId(request.CompanyId);

                if (dbConfig == null)
                {
                    return ServiceResult<IEnumerable<AttandanceSynchronization>>
                        .FailureResult("No database configuration assigned for this company");
                }

                // Decrypt database password
                var decryptedPassword =
                    EncryptionHelper.Decrypt(dbConfig.DatabasePassword);

                // Prepare database connection DTO
                var connectionDto = new DatabaseConnectionDto
                {
                    DatabaseIP = dbConfig.DatabaseIP,
                    DatabaseUserId = dbConfig.DatabaseUserId,
                    DatabasePassword = decryptedPassword,
                    DatabaseName = dbConfig.DatabaseName
                };

                // Build connection string
                var connectionString =
                    BuildConnectionString(connectionDto);

                // Query the dynamically assigned database
                using (var context =
                    new DynamicDbContext(connectionString))
                {
                    IQueryable<AttandanceSynchronization> query =
                        context.AttandanceSynchronizations
                            .AsNoTracking();

                    // Apply optional date filters
                    if (fromDate.HasValue)
                    {
                        query = query
                            .Where(a => a.FromDate >= fromDate.Value);
                    }

                    if (toDate.HasValue)
                    {
                        query = query
                            .Where(a => a.ToDate <= toDate.Value);
                    }

                    // Execute query and return results
                    var data =
                        query.OrderByDescending(a => a.Id)
                             .ToList();

                    return ServiceResult<IEnumerable<AttandanceSynchronization>>
                        .SuccessResult(data, "Data retrieved successfully");
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<AttandanceSynchronization>>
                    .FailureResult($"Error retrieving data: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds a SQL Server connection string using explicit credentials.
        /// </summary>
        /// <param name="config">Database connection configuration.</param>
        /// <returns>Formatted SQL connection string.</returns>
        public string BuildConnectionString(DatabaseConnectionDto config)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = config.DatabaseIP,
                InitialCatalog = config.DatabaseName,
                UserID = config.DatabaseUserId,
                Password = config.DatabasePassword,
                IntegratedSecurity = false,
                ConnectTimeout = 30,
                Encrypt = false,
                TrustServerCertificate = true
            };

            return builder.ConnectionString;
        }
    }
}
