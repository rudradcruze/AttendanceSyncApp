using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Services.AttandanceSync
{
    /// <summary>
    /// Provides services for connecting to and retrieving data from
    /// dynamically configured external databases.
    /// </summary>
    public class DynamicDatabaseService : IDynamicDatabaseService
    {
        /// <summary>
        /// Unit of work for authentication and configuration-related data.
        /// </summary>
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes the DynamicDatabaseService.
        /// </summary>
        /// <param name="unitOfWork">
        /// Unit of work used to access requests and database configurations.
        /// </param>
        public DynamicDatabaseService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Tests a database connection using the provided configuration.
        /// </summary>
        /// <param name="config">Database connection configuration.</param>
        /// <returns>
        /// True if the connection is successful; otherwise, failure result.
        /// </returns>
        public ServiceResult<bool> TestConnection(DatabaseConnectionDto config)
        {
            try
            {
                // Build connection string from configuration
                var connectionString = BuildConnectionString(config);

                // Attempt to open SQL connection
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
        /// configured company database.
        /// </summary>
        /// <param name="requestId">Attendance sync request ID.</param>
        /// <param name="fromDate">Optional start date filter.</param>
        /// <param name="toDate">Optional end date filter.</param>
        /// <returns>Filtered attendance synchronization records.</returns>
        public ServiceResult<IEnumerable<AttandanceSynchronization>> GetAttendanceData(
            int requestId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                // Retrieve the sync request to determine CompanyId
                var request = _unitOfWork.AttandanceSyncRequests
                    .GetById(requestId);

                if (request == null)
                {
                    return ServiceResult<IEnumerable<AttandanceSynchronization>>
                        .FailureResult("Request not found");
                }

                // Retrieve database configuration for the company
                var dbConfig = _unitOfWork.DatabaseConfigurations
                    .GetByCompanyId(request.CompanyId);

                if (dbConfig == null)
                {
                    return ServiceResult<IEnumerable<AttandanceSynchronization>>
                        .FailureResult(
                            "No database configuration assigned for this company");
                }

                // Decrypt stored database password
                var decryptedPassword =
                    EncryptionHelper.Decrypt(dbConfig.DatabasePassword);

                // Prepare connection DTO
                var connectionDto = new DatabaseConnectionDto
                {
                    DatabaseIP = dbConfig.DatabaseIP,
                    DatabaseUserId = dbConfig.DatabaseUserId,
                    DatabasePassword = decryptedPassword,
                    DatabaseName = dbConfig.DatabaseName
                };

                // Build dynamic connection string
                var connectionString = BuildConnectionString(connectionDto);

                // Query the external (dynamic) database
                using (var context = new DynamicDbContext(connectionString))
                {
                    IQueryable<AttandanceSynchronization> query =
                        context.AttandanceSynchronizations.AsNoTracking();

                    // Apply optional date filters
                    if (fromDate.HasValue)
                    {
                        query = query.Where(a =>
                            a.FromDate >= fromDate.Value);
                    }

                    if (toDate.HasValue)
                    {
                        query = query.Where(a =>
                            a.ToDate <= toDate.Value);
                    }

                    // Execute query
                    var data = query
                        .OrderByDescending(a => a.Id)
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
        /// Builds a SQL Server connection string from the given configuration.
        /// </summary>
        /// <param name="config">Database connection details.</param>
        /// <returns>SQL connection string.</returns>
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
