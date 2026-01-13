using System;

namespace AttandanceSyncApp.Models.DTOs.Sync
{
    public class SyncRequestDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string CompanyName { get; set; }
        public string ToolName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
