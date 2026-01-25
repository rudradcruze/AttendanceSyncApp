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

            // Get ServerIp ID from IP address to check access
            var serverIpRecord = _unitOfWork.ServerIps.GetByIpAddress(serverIp);
            if (serverIpRecord == null)
            {
                return databases; // No filtering if server not found
            }

            // Get accessible databases from DatabaseAccess table
            var accessibleDatabases = new HashSet<string>(
                _unitOfWork.DatabaseAccess
                    .GetAccessibleDatabasesByServerId(serverIpRecord.Id)
                    .Select(da => da.DatabaseName)
            );

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
                            var dbName = reader.GetString(0);
                            // Only include if in accessible list
                            if (accessibleDatabases.Contains(dbName))
                            {
                                databases.Add(dbName);
                            }
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

        public ServiceResult<IEnumerable<ProblematicGarbageDto>> ScanDatabaseForProblematic(int serverIpId, string databaseName)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<ProblematicGarbageDto>>.FailureResult("Server IP not found");
                }

                var problematicData = ScanDatabaseForProblematicSalary(serverIp.IpAddress, serverIp.DatabaseUser, serverIp.DatabasePassword, databaseName);
                return ServiceResult<IEnumerable<ProblematicGarbageDto>>.SuccessResult(problematicData);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ProblematicGarbageDto>>.FailureResult($"Failed to scan database: {ex.Message}");
            }
        }

        public ServiceResult<ProblematicScanResultDto> ScanAllProblematicDatabases()
        {
            try
            {
                var result = new ProblematicScanResultDto();
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
                                var problematicData = ScanDatabaseForProblematicSalary(serverIp.IpAddress, serverIp.DatabaseUser, serverIp.DatabasePassword, dbName);
                                result.ProblematicData.AddRange(problematicData);
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

                result.TotalProblematicRecords = result.ProblematicData.Count;
                result.Summary = $"Scanned {result.TotalServers} servers, {result.TotalDatabases} databases. Found {result.TotalProblematicRecords} problematic records.";

                return ServiceResult<ProblematicScanResultDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<ProblematicScanResultDto>.FailureResult($"Failed to scan databases: {ex.Message}");
            }
        }

        private List<ProblematicGarbageDto> ScanDatabaseForProblematicSalary(string serverIp, string userId, string encryptedPassword, string databaseName)
        {
            var problematicData = new List<ProblematicGarbageDto>();
            var connectionString = BuildConnectionString(serverIp, userId, encryptedPassword, databaseName);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if all required tables and columns exist
                var checkQuery = @"
                    SELECT 1
                    FROM sys.tables t1
                    INNER JOIN sys.tables t2 ON t2.name = 'PromotionIncrements'
                    INNER JOIN sys.tables t3 ON t3.name = 'Confirmations'
                    INNER JOIN sys.columns ec1 ON t1.object_id = ec1.object_id AND ec1.name = 'Id'
                    INNER JOIN sys.columns ec2 ON t1.object_id = ec2.object_id AND ec2.name = 'FirstName'
                    INNER JOIN sys.columns ec3 ON t1.object_id = ec3.object_id AND ec3.name = 'BasicSalary'
                    INNER JOIN sys.columns pc1 ON t2.object_id = pc1.object_id AND pc1.name = 'EmployeeId'
                    INNER JOIN sys.columns pc2 ON t2.object_id = pc2.object_id AND pc2.name = 'NewBasicSalary'
                    INNER JOIN sys.columns pc3 ON t2.object_id = pc3.object_id AND pc3.name = 'EffectiveDate'
                    INNER JOIN sys.columns cc1 ON t3.object_id = cc1.object_id AND cc1.name = 'Id'
                    INNER JOIN sys.columns cc2 ON t3.object_id = cc2.object_id AND cc2.name = 'EmployeeId'
                    INNER JOIN sys.columns cc3 ON t3.object_id = cc3.object_id AND cc3.name = 'NewBasicSalary'
                    WHERE t1.name = 'Employees'";

                bool hasAllTables = false;
                using (var checkCommand = new SqlCommand(checkQuery, connection))
                {
                    var result = checkCommand.ExecuteScalar();
                    hasAllTables = result != null;
                }

                if (!hasAllTables)
                {
                    return problematicData; // Return empty list if tables don't exist
                }

                // Query for salary mismatches from PromotionIncrements
                var promotionQuery = @"
                    SELECT
                        e.Id as EmployeeId,
                        e.FirstName,
                        'PromotionIncrements' as IssueTableName,
                        e.BasicSalary as CurrentBasicSalary,
                        lp.NewBasicSalary as ExpectedBasicSalary
                    FROM dbo.Employees e
                    INNER JOIN (
                        SELECT
                            EmployeeId,
                            NewBasicSalary,
                            EffectiveDate,
                            ROW_NUMBER() OVER (PARTITION BY EmployeeId ORDER BY EffectiveDate DESC) as rn
                        FROM dbo.PromotionIncrements
                    ) lp ON e.Id = lp.EmployeeId AND lp.rn = 1
                    WHERE e.BasicSalary != lp.NewBasicSalary";

                using (var command = new SqlCommand(promotionQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            problematicData.Add(new ProblematicGarbageDto
                            {
                                ServerIp = serverIp,
                                DatabaseName = databaseName,
                                EmployeeId = reader.GetInt32(0),
                                EmployeeName = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1),
                                IssueTableName = reader.GetString(2),
                                CurrentBasicSalary = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                                ExpectedBasicSalary = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4)
                            });
                        }
                    }
                }

                // Query for salary mismatches from Confirmations (only when no PromotionIncrements exist)
                var confirmationQuery = @"
                    SELECT
                        e.Id as EmployeeId,
                        e.FirstName,
                        'Confirmations' as IssueTableName,
                        e.BasicSalary as CurrentBasicSalary,
                        lc.NewBasicSalary as ExpectedBasicSalary
                    FROM dbo.Employees e
                    INNER JOIN (
                        SELECT
                            EmployeeId,
                            NewBasicSalary,
                            ROW_NUMBER() OVER (PARTITION BY EmployeeId ORDER BY Id DESC) as rn
                        FROM dbo.Confirmations
                    ) lc ON e.Id = lc.EmployeeId AND lc.rn = 1
                    LEFT JOIN (
                        SELECT
                            EmployeeId,
                            NewBasicSalary,
                            EffectiveDate,
                            ROW_NUMBER() OVER (PARTITION BY EmployeeId ORDER BY EffectiveDate DESC) as rn
                        FROM dbo.PromotionIncrements
                    ) lp ON e.Id = lp.EmployeeId AND lp.rn = 1
                    WHERE lp.EmployeeId IS NULL
                      AND e.BasicSalary != lc.NewBasicSalary";

                using (var command = new SqlCommand(confirmationQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            problematicData.Add(new ProblematicGarbageDto
                            {
                                ServerIp = serverIp,
                                DatabaseName = databaseName,
                                EmployeeId = reader.GetInt32(0),
                                EmployeeName = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1),
                                IssueTableName = reader.GetString(2),
                                CurrentBasicSalary = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                                ExpectedBasicSalary = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4)
                            });
                        }
                    }
                }
            }

            return problematicData;
        }
    }
}
