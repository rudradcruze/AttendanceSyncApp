using System.Collections.Generic;
using AttandanceSyncApp.Models.ConcurrentSimulation;

namespace AttandanceSyncApp.Models.DTOs.ConcurrentSimulation
{
    /// <summary>
    /// DTO for server IP display
    /// </summary>
    public class ServerIpDto
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// DTO for database list display
    /// </summary>
    public class DatabaseListDto
    {
        public string DatabaseName { get; set; }
    }

    /// <summary>
    /// Request DTO for fetching period end data
    /// </summary>
    public class PeriodEndDataRequestDto
    {
        public int ServerIpId { get; set; }
        public string DatabaseName { get; set; }
    }

    /// <summary>
    /// Request DTO for hitting concurrent inserts
    /// </summary>
    public class HitConcurrentRequestDto
    {
        public int ServerIpId { get; set; }
        public string DatabaseName { get; set; }
    }

    /// <summary>
    /// Response DTO for hit concurrent operation
    /// </summary>
    public class HitConcurrentResponseDto
    {
        public int TotalRecords { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; }
    }
}
