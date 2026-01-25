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
    public class DynamicDatabaseService : IDynamicDatabaseService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public DynamicDatabaseService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<bool> TestConnection(DatabaseConnectionDto config)
        {
            try
            {
                var connectionString = BuildConnectionString(config);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return ServiceResult<bool>.SuccessResult(true, "Connection successful");
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"Connection failed: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<AttandanceSynchronization>> GetAttendanceData(
            int requestId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                // Get the request to find the CompanyId
                var request = _unitOfWork.AttandanceSyncRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult<IEnumerable<AttandanceSynchronization>>
                        .FailureResult("Request not found");
                }

                // Get the database configuration for this Company
                var dbConfig = _unitOfWork.DatabaseConfigurations.GetByCompanyId(request.CompanyId);
                if (dbConfig == null)
                {
                    return ServiceResult<IEnumerable<AttandanceSynchronization>>
                        .FailureResult("No database configuration assigned for this company");
                }

                // Decrypt the password
                var decryptedPassword = EncryptionHelper.Decrypt(dbConfig.DatabasePassword);

                var connectionDto = new DatabaseConnectionDto
                {
                    DatabaseIP = dbConfig.DatabaseIP,
                    DatabaseUserId = dbConfig.DatabaseUserId,
                    DatabasePassword = decryptedPassword,
                    DatabaseName = dbConfig.DatabaseName
                };

                var connectionString = BuildConnectionString(connectionDto);

                // Query the dynamic database
                using (var context = new DynamicDbContext(connectionString))
                {
                    IQueryable<AttandanceSynchronization> query =
                        context.AttandanceSynchronizations.AsNoTracking();

                    if (fromDate.HasValue)
                    {
                        query = query.Where(a => a.FromDate >= fromDate.Value);
                    }

                    if (toDate.HasValue)
                    {
                        query = query.Where(a => a.ToDate <= toDate.Value);
                    }

                    var data = query.OrderByDescending(a => a.Id).ToList();
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