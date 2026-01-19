using System;

namespace AttandanceSyncApp.Models.DTOs.Admin
{
    /// <summary>
    /// DTO for displaying database assignments in the admin list view.
    /// </summary>
    public class DatabaseAssignListDto
    {
        public int Id { get; set; }
        public int CompanyRequestId { get; set; }

        // Request Info
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string EmployeeName { get; set; }
        public string CompanyName { get; set; }
        public string ToolName { get; set; }

        // Assignment Info
        public int AssignedBy { get; set; }
        public string AssignedByName { get; set; }
        public DateTime AssignedAt { get; set; }

        // Database Config Info
        public int DatabaseConfigurationId { get; set; }
        public string DatabaseIP { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUserId { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
