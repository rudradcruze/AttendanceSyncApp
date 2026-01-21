using System.Collections.Generic;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;

namespace AttandanceSyncApp.Services.Interfaces.SalaryGarbge
{
    public interface ISalaryGarbgeScanService
    {
        /// <summary>
        /// Gets all active server IPs for scanning
        /// </summary>
        ServiceResult<IEnumerable<ServerIpDto>> GetActiveServerIps();

        /// <summary>
        /// Gets all databases on a specific server IP
        /// </summary>
        ServiceResult<IEnumerable<string>> GetDatabasesOnServer(int serverIpId);

        /// <summary>
        /// Scans a specific database for garbage data
        /// </summary>
        ServiceResult<IEnumerable<GarbageDataDto>> ScanDatabase(int serverIpId, string databaseName);

        /// <summary>
        /// Scans all databases across all server IPs and returns garbage data
        /// </summary>
        ServiceResult<GarbageScanResultDto> ScanAllDatabases();
    }
}
