using System;

namespace AttandanceSyncApp.Models.DTOs.Admin
{
    public class RequestListDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int ToolId { get; set; }
        public string ToolName { get; set; }
        public int SessionId { get; set; }
        public int? ExternalSyncId { get; set; }
        public bool? IsSuccessful { get; set; }
        public string Status { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool HasDatabaseConfig { get; set; }
    }
}
