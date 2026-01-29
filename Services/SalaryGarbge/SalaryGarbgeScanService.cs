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
    /// <summary>
    /// Service responsible for scanning salary-related garbage and
    /// problematic data across databases. Supports single-database
    /// and multi-server scanning with access control validation.
    /// </summary>
    public class SalaryGarbgeScanService : ISalaryGarbgeScanService
    {
        /// Unit of work for accessing repositories.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of SalaryGarbgeScanService.
        /// </summary>
        /// <param name="unitOfWork">Authentication unit of work.</param>
        public SalaryGarbgeScanService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all active server IPs available for scanning.
        /// </summary>
        /// <returns>List of active server IPs.</returns>
        public ServiceResult<IEnumerable<ServerIpDto>> GetActiveServerIps()
        {
            try
            {
                // Fetch active servers and map to DTOs
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

                return ServiceResult<IEnumerable<ServerIpDto>>
                    .SuccessResult(serverIps);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ServerIpDto>>
                    .FailureResult($"Failed to get server IPs: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves accessible databases for a specific server.
        /// </summary>
        /// <param name="serverIpId">Server IP identifier.</param>
        /// <returns>List of database names.</returns>
        public ServiceResult<IEnumerable<string>> GetDatabasesOnServer(int serverIpId)
        {
            try
            {
                // Validate server IP
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<string>>
                        .FailureResult("Server IP not found");
                }

                // Retrieve databases from SQL Server
                var databases = GetDatabasesFromServer(
                    serverIp.IpAddress,
                    serverIp.DatabaseUser,
                    serverIp.DatabasePassword);

                return ServiceResult<IEnumerable<string>>
                    .SuccessResult(databases);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<string>>
                    .FailureResult($"Failed to get databases: {ex.Message}");
            }
        }

        /// <summary>
        /// Scans a single database for garbage salary data.
        /// </summary>
        /// <param name="serverIpId">Server IP identifier.</param>
        /// <param name="databaseName">Database name.</param>
        /// <returns>Detected garbage salary records.</returns>
        public ServiceResult<IEnumerable<GarbageDataDto>>
            ScanDatabase(int serverIpId, string databaseName)
        {
            try
            {
                // Validate server IP
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<GarbageDataDto>>
                        .FailureResult("Server IP not found");
                }

                // Execute garbage scan
                var garbageData = ScanDatabaseForGarbage(
                    serverIp.IpAddress,
                    serverIp.DatabaseUser,
                    serverIp.DatabasePassword,
                    databaseName);

                return ServiceResult<IEnumerable<GarbageDataDto>>
                    .SuccessResult(garbageData);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<GarbageDataDto>>
                    .FailureResult($"Failed to scan database: {ex.Message}");
            }
        }

        /// <summary>
        /// Scans all accessible databases across all active servers
        /// for garbage salary data.
        /// </summary>
        /// <returns>Aggregated scan result.</returns>
        public ServiceResult<GarbageScanResultDto> ScanAllDatabases()
        {
            try
            {
                var result = new GarbageScanResultDto();

                // Retrieve all active servers
                var serverIps = _unitOfWork.ServerIps
                    .GetAll()
                    .Where(s => s.IsActive)
                    .ToList();

                result.TotalServers = serverIps.Count;

                foreach (var serverIp in serverIps)
                {
                    try
                    {
                        // Retrieve accessible databases per server
                        var databases = GetDatabasesFromServer(
                            serverIp.IpAddress,
                            serverIp.DatabaseUser,
                            serverIp.DatabasePassword);

                        result.TotalDatabases += databases.Count;

                        foreach (var dbName in databases)
                        {
                            try
                            {
                                // Scan each database for garbage data
                                var garbageData = ScanDatabaseForGarbage(
                                    serverIp.IpAddress,
                                    serverIp.DatabaseUser,
                                    serverIp.DatabasePassword,
                                    dbName);

                                result.GarbageData.AddRange(garbageData);
                            }
                            catch
                            {
                                // Skip databases with scan errors
                            }
                        }
                    }
                    catch
                    {
                        // Skip servers with connection errors
                    }
                }

                result.TotalGarbageRecords = result.GarbageData.Count;
                result.Summary =
                    $"Scanned {result.TotalServers} servers, " +
                    $"{result.TotalDatabases} databases. " +
                    $"Found {result.TotalGarbageRecords} garbage records.";

                return ServiceResult<GarbageScanResultDto>
                    .SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<GarbageScanResultDto>
                    .FailureResult($"Failed to scan databases: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds a SQL Server connection string.
        /// </summary>
        private string BuildConnectionString(
            string serverIp,
            string userId,
            string encryptedPassword,
            string databaseName = "master")
        {
            // Decrypt stored password
            var decryptedPassword =
                EncryptionHelper.Decrypt(encryptedPassword);

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

        /// <summary>
        /// Retrieves all accessible databases from a SQL Server instance.
        /// </summary>
        private List<string> GetDatabasesFromServer(
            string serverIp,
            string userId,
            string encryptedPassword)
        {
            var databases = new List<string>();
            var connectionString =
                BuildConnectionString(serverIp, userId, encryptedPassword);

            // Resolve server IP record for access filtering
            var serverIpRecord =
                _unitOfWork.ServerIps.GetByIpAddress(serverIp);

            if (serverIpRecord == null)
                return databases;

            // Retrieve database access permissions
            var accessibleDatabases = new HashSet<string>(
                _unitOfWork.DatabaseAccess
                    .GetAccessibleDatabasesByServerId(serverIpRecord.Id)
                    .Select(da => da.DatabaseName)
            );

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Query online user databases only
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
                        var dbName = reader.GetString(0);

                        // Include only permitted databases
                        if (accessibleDatabases.Contains(dbName))
                        {
                            databases.Add(dbName);
                        }
                    }
                }
            }

            return databases;
        }

        /// <summary>
        /// Scans a database for garbage salary records.
        /// </summary>
        private List<GarbageDataDto> ScanDatabaseForGarbage(
            string serverIp,
            string userId,
            string encryptedPassword,
            string databaseName)
        {
            var garbageData = new List<GarbageDataDto>();
            var connectionString =
                BuildConnectionString(serverIp, userId, encryptedPassword, databaseName);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Validate Employees table and required columns
                var checkTableQuery = @"
                    SELECT 1
                    FROM sys.tables t
                    INNER JOIN sys.columns c1 ON t.object_id = c1.object_id AND c1.name = 'Id'
                    INNER JOIN sys.columns c2 ON t.object_id = c2.object_id AND c2.name = 'EmployeeId'
                    INNER JOIN sys.columns c3 ON t.object_id = c3.object_id AND c3.name = 'FirstName'
                    INNER JOIN sys.columns c4 ON t.object_id = c4.object_id AND c4.name = 'GradeScaleId'
                    INNER JOIN sys.columns c5 ON t.object_id = c5.object_id AND c5.name = 'BasicSalary'
                    WHERE t.name = 'Employees'";

                using (var checkCommand =
                    new SqlCommand(checkTableQuery, connection))
                {
                    if (checkCommand.ExecuteScalar() == null)
                        return garbageData;
                }

                // Detect invalid salary and grade scale data
                var query = @"
                    SELECT
                        Id,
                        EmployeeId AS EmployeeCode,
                        FirstName,
                        GradeScaleId,
                        BasicSalary
                    FROM dbo.Employees
                    WHERE GradeScaleId = 0
                       OR GradeScaleId IS NULL
                       OR BasicSalary = 0
                       OR BasicSalary IS NULL";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var employeeId = reader.GetInt32(0);
                        var employeeCode =
                            reader.IsDBNull(1) ? null : reader.GetValue(1).ToString();
                        var firstName =
                            reader.IsDBNull(2) ? "Unknown" : reader.GetString(2);
                        var gradeScaleId =
                            reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);
                        var basicSalary =
                            reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4);

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
                                EmployeeCode = employeeCode,
                                EmployeeName = firstName,
                                Problem = problem
                            });
                        }
                    }
                }
            }

            return garbageData;
        }

        /// <summary>
        /// Scans a database for problematic salary mismatches.
        /// </summary>
        public ServiceResult<IEnumerable<ProblematicGarbageDto>>
            ScanDatabaseForProblematic(int serverIpId, string databaseName)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(serverIpId);
                if (serverIp == null)
                {
                    return ServiceResult<IEnumerable<ProblematicGarbageDto>>
                        .FailureResult("Server IP not found");
                }

                var problematicData =
                    ScanDatabaseForProblematicSalary(
                        serverIp.IpAddress,
                        serverIp.DatabaseUser,
                        serverIp.DatabasePassword,
                        databaseName);

                return ServiceResult<IEnumerable<ProblematicGarbageDto>>
                    .SuccessResult(problematicData);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ProblematicGarbageDto>>
                    .FailureResult($"Failed to scan database: {ex.Message}");
            }
        }

        /// <summary>
        /// Scans all databases for problematic salary inconsistencies.
        /// </summary>
        public ServiceResult<ProblematicScanResultDto>
            ScanAllProblematicDatabases()
        {
            try
            {
                var result = new ProblematicScanResultDto();

                var serverIps = _unitOfWork.ServerIps
                    .GetAll()
                    .Where(s => s.IsActive)
                    .ToList();

                result.TotalServers = serverIps.Count;

                foreach (var serverIp in serverIps)
                {
                    try
                    {
                        var databases = GetDatabasesFromServer(
                            serverIp.IpAddress,
                            serverIp.DatabaseUser,
                            serverIp.DatabasePassword);

                        result.TotalDatabases += databases.Count;

                        foreach (var dbName in databases)
                        {
                            try
                            {
                                var problematicData =
                                    ScanDatabaseForProblematicSalary(
                                        serverIp.IpAddress,
                                        serverIp.DatabaseUser,
                                        serverIp.DatabasePassword,
                                        dbName);

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
                        // Skip servers with errors
                    }
                }

                result.TotalProblematicRecords =
                    result.ProblematicData.Count;

                result.Summary =
                    $"Scanned {result.TotalServers} servers, " +
                    $"{result.TotalDatabases} databases. " +
                    $"Found {result.TotalProblematicRecords} problematic records.";

                return ServiceResult<ProblematicScanResultDto>
                    .SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<ProblematicScanResultDto>
                    .FailureResult($"Failed to scan databases: {ex.Message}");
            }
        }

        /// <summary>
        /// Detects salary mismatches between Employees and PromotionIncrements.
        /// </summary>
        private List<ProblematicGarbageDto>
            ScanDatabaseForProblematicSalary(
                string serverIp,
                string userId,
                string encryptedPassword,
                string databaseName)
        {
            var problematicData = new List<ProblematicGarbageDto>();
            var connectionString =
                BuildConnectionString(serverIp, userId, encryptedPassword, databaseName);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Validate required tables and columns
                var checkQuery = @"
                    SELECT 1
                    FROM sys.tables t1
                    INNER JOIN sys.tables t2 ON t2.name = 'PromotionIncrements'
                    INNER JOIN sys.tables t3 ON t3.name = 'Confirmations'
                    INNER JOIN sys.columns ec1 ON t1.object_id = ec1.object_id AND ec1.name = 'Id'
                    INNER JOIN sys.columns ec2 ON t1.object_id = ec2.object_id AND ec2.name = 'EmployeeId'
                    INNER JOIN sys.columns ec3 ON t1.object_id = ec3.object_id AND ec3.name = 'FirstName'
                    INNER JOIN sys.columns ec4 ON t1.object_id = ec4.object_id AND ec4.name = 'BasicSalary'
                    INNER JOIN sys.columns pc1 ON t2.object_id = pc1.object_id AND pc1.name = 'EmployeeId'
                    INNER JOIN sys.columns pc2 ON t2.object_id = pc2.object_id AND pc2.name = 'NewBasicSalary'
                    INNER JOIN sys.columns pc3 ON t2.object_id = pc3.object_id AND pc3.name = 'EffectiveDate'
                    INNER JOIN sys.columns cc1 ON t3.object_id = cc1.object_id AND cc1.name = 'Id'
                    INNER JOIN sys.columns cc2 ON t3.object_id = cc2.object_id AND cc2.name = 'EmployeeId'
                    INNER JOIN sys.columns cc3 ON t3.object_id = cc3.object_id AND cc3.name = 'NewBasicSalary'
                    WHERE t1.name = 'Employees'";

                using (var checkCommand =
                    new SqlCommand(checkQuery, connection))
                {
                    if (checkCommand.ExecuteScalar() == null)
                        return problematicData;
                }

                // Detect promotion-based salary mismatches
                var promotionQuery = @"
                    SELECT
                        e.Id AS EmployeeId,
                        e.EmployeeId AS EmployeeCode,
                        e.FirstName,
                        'PromotionIncrements' AS IssueTableName,
                        e.BasicSalary AS CurrentBasicSalary,
                        lp.NewBasicSalary AS ExpectedBasicSalary
                    FROM dbo.Employees e
                    INNER JOIN (
                        SELECT
                            EmployeeId,
                            NewBasicSalary,
                            EffectiveDate,
                            ROW_NUMBER() OVER
                                (PARTITION BY EmployeeId ORDER BY EffectiveDate DESC) AS rn
                        FROM dbo.PromotionIncrements
                    ) lp ON e.Id = lp.EmployeeId AND lp.rn = 1
                    WHERE e.BasicSalary <> lp.NewBasicSalary";

                using (var command = new SqlCommand(promotionQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        problematicData.Add(new ProblematicGarbageDto
                        {
                            ServerIp = serverIp,
                            DatabaseName = databaseName,
                            EmployeeId = reader.GetInt32(0),
                            EmployeeCode = reader.IsDBNull(1)
                                ? null
                                : reader.GetValue(1).ToString(),
                            EmployeeName = reader.IsDBNull(2)
                                ? "Unknown"
                                : reader.GetString(2),
                            IssueTableName = reader.GetString(3),
                            CurrentBasicSalary = reader.IsDBNull(4)
                                ? 0
                                : reader.GetDecimal(4),
                            ExpectedBasicSalary = reader.IsDBNull(5)
                                ? 0
                                : reader.GetDecimal(5)
                        });
                    }
                }
            }

            return problematicData;
        }
    }
}
