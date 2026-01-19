using System;

namespace AttandanceSyncApp.Models.DTOs.CompanyRequest
{
    public class CompanyRequestListDto
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
        public string Status { get; set; }
        public string StatusText { get; set; }
        public bool IsCancelled { get; set; }
        public bool CanProcess { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
