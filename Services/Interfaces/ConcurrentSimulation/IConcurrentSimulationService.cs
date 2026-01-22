using System.Collections.Generic;
using AttandanceSyncApp.Models.ConcurrentSimulation;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.ConcurrentSimulation;

namespace AttandanceSyncApp.Services.Interfaces.ConcurrentSimulation
{
    public interface IConcurrentSimulationService
    {
        /// <summary>
        /// Gets all active server IPs
        /// </summary>
        ServiceResult<IEnumerable<ServerIpDto>> GetAllServerIps();

        /// <summary>
        /// Gets all databases for a given server IP
        /// </summary>
        ServiceResult<IEnumerable<DatabaseListDto>> GetDatabasesForServer(int serverIpId);

        /// <summary>
        /// Gets period end data from the specified database
        /// </summary>
        ServiceResult<IEnumerable<PeriodEndProcessEntry>> GetPeriodEndData(int serverIpId, string databaseName);

        /// <summary>
        /// Inserts all entries concurrently into PeriodEndProcessRequest table
        /// </summary>
        ServiceResult<HitConcurrentResponseDto> HitConcurrent(HitConcurrentRequestDto request);
    }
}
