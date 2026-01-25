using System;

namespace AttandanceSyncApp.Models.DTOs.AttandanceSync
{
    public class CompanyRequestDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string EmployeeName { get; set; }
        public string CompanyName { get; set; }
        public string ToolName { get; set; }
        public string Status { get; set; }
        public string StatusText { get; set; }
        public bool IsCancelled { get; set; }
        public bool CanCancel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
