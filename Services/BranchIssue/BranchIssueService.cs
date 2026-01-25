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
    public class BranchIssueService : IBranchIssueService
    {
        private readonly IAuthUnitOfWork _unitOfWork;
        private readonly IBranchIssueRepository _repository;

        public BranchIssueService(IAuthUnitOfWork unitOfWork, IBranchIssueRepository repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public ServiceResult<IEnumerable<ServerIpDto>> GetAllServerIps()
        {
            try
            {
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
                return ServiceResult<IEnumerable<ServerIpDto>>.FailureResult($"Error fetching server IPs: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<DatabaseListDto>> GetDatabasesForServer(int serverIpId)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<DatabaseListDto>>.FailureResult("Server IP not found");
                }

                // Get accessible databases from DatabaseAccess table
                var accessibleDatabases = new HashSet<string>(
                    _unitOfWork.DatabaseAccess
                        .GetAccessibleDatabasesByServerId(serverIpId)
                        .Select(da => da.DatabaseName)
                );

                if (!accessibleDatabases.Any())
                {
                    return ServiceResult<IEnumerable<DatabaseListDto>>.SuccessResult(new List<DatabaseListDto>());
                }

                var decryptedPassword = EncryptionHelper.Decrypt(serverIp.DatabasePassword);
                var connectionString = BuildConnectionString(serverIp.IpAddress, serverIp.DatabaseUser, decryptedPassword, "master");

                var databases = new List<DatabaseListDto>();

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var dbName = reader.GetString(0);
                                if (accessibleDatabases.Contains(dbName))
                                {
                                    databases.Add(new DatabaseListDto { DatabaseName = dbName });
                                }
                            }
                        }
                    }
                }

                return ServiceResult<IEnumerable<DatabaseListDto>>.SuccessResult(databases);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<DatabaseListDto>>.FailureResult($"Error fetching databases: {ex.Message}");
            }
        }

        public ServiceResult<string> GetLastMonthDate(int serverIpId, string databaseName)
        {
            try
            {
                var connectionString = GetConnectionString(serverIpId, databaseName);
                if (string.IsNullOrEmpty(connectionString))
                    return ServiceResult<string>.FailureResult("Could not build connection string");

                var date = _repository.GetLastMonthDate(connectionString);
                return ServiceResult<string>.SuccessResult(date.ToString("yyyy-MM-dd"));
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"Error getting last month date: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<ProblemBranchDto>> GetProblemBranches(int serverIpId, string databaseName, string monthStartDate, string locationId)
        {
            try
            {
                var connectionString = GetConnectionString(serverIpId, databaseName);
                if (string.IsNullOrEmpty(connectionString))
                    return ServiceResult<IEnumerable<ProblemBranchDto>>.FailureResult("Could not build connection string");

                if (!DateTime.TryParse(monthStartDate, out DateTime parsedDate))
                {
                    return ServiceResult<IEnumerable<ProblemBranchDto>>.FailureResult("Invalid date format");
                }

                var branches = _repository.GetProblemBranches(connectionString, parsedDate, locationId);
                
                var dtos = branches.Select(b => new ProblemBranchDto
                {
                    PeriodFrom = b.PeriodFrom,
                    BranchCode = b.BranchCode,
                    BranchName = b.BranchName,
                    Remarks = b.Remarks
                }).ToList();

                return ServiceResult<IEnumerable<ProblemBranchDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ProblemBranchDto>>.FailureResult($"Error fetching problem branches: {ex.Message}");
            }
        }

        public ServiceResult<string> ReprocessBranch(ReprocessBranchRequestDto request)
        {
            try
            {
                var connectionString = GetConnectionString(request.ServerIpId, request.DatabaseName);
                if (string.IsNullOrEmpty(connectionString))
                    return ServiceResult<string>.FailureResult("Could not build connection string");

                _repository.ReprocessBranch(connectionString, request.BranchCode, request.Month, request.PrevMonth);
                
                return ServiceResult<string>.SuccessResult("Branch reprocessed successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"Error reprocessing branch: {ex.Message}");
            }
        }

        private string GetConnectionString(int serverIpId, string databaseName)
        {
            var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
            if (serverIp == null) return null;

            var decryptedPassword = EncryptionHelper.Decrypt(serverIp.DatabasePassword);
            return BuildConnectionString(serverIp.IpAddress, serverIp.DatabaseUser, decryptedPassword, databaseName);
        }

        private string BuildConnectionString(string ipAddress, string userId, string password, string databaseName)
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
