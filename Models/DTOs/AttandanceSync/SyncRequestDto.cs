using System;

namespace AttandanceSyncApp.Models.DTOs.AttandanceSync
{
    public class SyncRequestDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string EmployeeName { get; set; }
        public string CompanyName { get; set; }
        public string ToolName { get; set; }
        public int? ExternalSyncId { get; set; }
        public bool? IsSuccessful { get; set; }
        public string Status { get; set; } // Computed from IsSuccessful
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
