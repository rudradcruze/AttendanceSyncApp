using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Services.SalaryGarbge
{
    public class SalaryGarbgeScanService : ISalaryGarbgeScanService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public SalaryGarbgeScanService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<IEnumerable<ServerIpDto>> GetActiveServerIps()
        {
            try
            {
                var serverIps = _unitOfWork.ServerIps.GetAll()
                    .Where(s => s.IsActive)
                    .Select(s => new ServerIpDto
                    {
                        Id = s.Id,
                        IpAddress = s.IpAddress,
                        DatabaseUser = s.DatabaseUser,
                        Description = s.Description,
                        IsActive = s.IsActive
                    })
                    .ToList();

                return ServiceResult<IEnumerable<ServerIpDto>>.SuccessResult(serverIps);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ServerIpDto>>.FailureResult($"Failed to get server IPs: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<string>> GetDatabasesOnServer(int serverIpId)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<string>>.FailureResult("Server IP not found");
                }

                var databases = GetDatabasesFromServer(serverIp.IpAddress, serverIp.DatabaseUser, serverIp.DatabasePassword);
                return ServiceResult<IEnumerable<string>>.SuccessResult(databases);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<string>>.FailureResult($"Failed to get databases: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<GarbageDataDto>> ScanDatabase(int serverIpId, string databaseName)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<GarbageDataDto>>.FailureResult("Server IP not found");
                }

                var garbageData = ScanDatabaseForGarbage(serverIp.IpAddress, serverIp.DatabaseUser, serverIp.DatabasePassword, databaseName);
                return ServiceResult<IEnumerable<GarbageDataDto>>.SuccessResult(garbageData);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<GarbageDataDto>>.FailureResult($"Failed to scan database: {ex.Message}");
            }
        }

        public ServiceResult<GarbageScanResultDto> ScanAllDatabases()
        {
            try
            {
                var result = new GarbageScanResultDto();
                var serverIps = _unitOfWork.ServerIps.GetAll().Where(s => s.IsActive).ToList();
                result.TotalServers = serverIps.Count;

                foreach (var serverIp in serverIps)
                {
                    try
                    {
                        var databases = GetDatabasesFromServer(serverIp.IpAddress, serverIp.DatabaseUser, serverIp.DatabasePassword);
                        result.TotalDatabases += databases.Count;

                        foreach (var dbName in databases)
                        {
                            try
                            {
                                var garbageData = ScanDatabaseForGarbage(serverIp.IpAddress, serverIp.DatabaseUser, serverIp.DatabasePassword, dbName);
                                result.GarbageData.AddRange(garbageData);
                            }
                            catch
                            {
                                // Skip databases with errors
                            }
                        }
                    }
                    catch
                    {
                        // Skip servers with connection errors
                    }
                }

                result.TotalGarbageRecords = result.GarbageData.Count;
                result.Summary = $"Scanned {result.TotalServers} servers, {result.TotalDatabases} databases. Found {result.TotalGarbageRecords} garbage records.";

                return ServiceResult<GarbageScanResultDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<GarbageScanResultDto>.FailureResult($"Failed to scan databases: {ex.Message}");
            }
        }

        private string BuildConnectionString(string serverIp, string userId, string encryptedPassword, string databaseName = "master")
        {
            var decryptedPassword = EncryptionHelper.Decrypt(encryptedPassword);

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = serverIp,
                InitialCatalog = databaseName,
                UserID = userId,
                Password = decryptedPassword,
                IntegratedSecurity = false,
                ConnectTimeout = 30,
                Encrypt = false,
                TrustServerCertificate = true
            };

            return builder.ConnectionString;
        }

        private List<string> GetDatabasesFromServer(string serverIp, string userId, string encryptedPassword)
        {
            var databases = new List<string>();
            var connectionString = BuildConnectionString(serverIp, userId, encryptedPassword);

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

        private List<GarbageDataDto> ScanDatabaseForGarbage(string serverIp, string userId, string encryptedPassword, string databaseName)
        {
            var garbageData = new List<GarbageDataDto>();
            var connectionString = BuildConnectionString(serverIp, userId, encryptedPassword, databaseName);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // First check if the Employees table exists with required columns
                var checkTableQuery = @"
                    SELECT 1
                    FROM sys.tables t
                    INNER JOIN sys.columns c1 ON t.object_id = c1.object_id AND c1.name = 'Id'
                    INNER JOIN sys.columns c2 ON t.object_id = c2.object_id AND c2.name = 'FirstName'
                    INNER JOIN sys.columns c3 ON t.object_id = c3.object_id AND c3.name = 'GradeScaleId'
                    INNER JOIN sys.columns c4 ON t.object_id = c4.object_id AND c4.name = 'BasicSalary'
                    WHERE t.name = 'Employees'";

                bool tableExists = false;
                using (var checkCommand = new SqlCommand(checkTableQuery, connection))
                {
                    var result = checkCommand.ExecuteScalar();
                    tableExists = result != null;
                }

                if (!tableExists)
                {
                    return garbageData; // Return empty list if table doesn't exist
                }

                // Query for garbage data
                var query = @"
                    SELECT
                        Id,
                        FirstName,
                        GradeScaleId,
                        BasicSalary,
                        CASE
                            WHEN GradeScaleId = 0 THEN 'GradeScaleId is 0'
                            WHEN GradeScaleId IS NULL THEN 'GradeScaleId is NULL'
                            WHEN BasicSalary = 0 THEN 'BasicSalary is 0'
                            WHEN BasicSalary IS NULL THEN 'BasicSalary is NULL'
                        END AS Problem
                    FROM dbo.Employees
                    WHERE GradeScaleId = 0
                       OR GradeScaleId IS NULL
                       OR BasicSalary = 0
                       OR BasicSalary IS NULL";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var employeeId = reader.GetInt32(0);
                            var firstName = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1);
                            var gradeScaleId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);
                            var basicSalary = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3);

                            // Handle multiple problems for same employee
                            var problems = new List<string>();
                            if (gradeScaleId == 0) problems.Add("GradeScaleId is 0");
                            if (gradeScaleId == null) problems.Add("GradeScaleId is NULL");
                            if (basicSalary == 0) problems.Add("BasicSalary is 0");
                            if (basicSalary == null) problems.Add("BasicSalary is NULL");

                            foreach (var problem in problems)
                            {
                                garbageData.Add(new GarbageDataDto
                                {
                                    ServerIp = serverIp,
                                    DatabaseName = databaseName,
                                    EmployeeId = employeeId,
                                    EmployeeName = firstName,
                                    Problem = problem
                                });
                            }
                        }
                    }
                }
            }

            return garbageData;
        }
    }
}
