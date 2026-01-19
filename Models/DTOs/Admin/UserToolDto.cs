using System;

namespace AttandanceSyncApp.Models.DTOs.Admin
{
    public class UserToolDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int ToolId { get; set; }
        public string ToolName { get; set; }
        public string AssignedByName { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
    }

    public class UserToolAssignDto
    {
        public int UserId { get; set; }
        public int ToolId { get; set; }
    }

    public class UserToolRevokeDto
    {
        public int UserId { get; set; }
        public int ToolId { get; set; }
    }

    public class AssignedToolDto
    {
        public int ToolId { get; set; }
        public string ToolName { get; set; }
        public string ToolDescription { get; set; }
        public string RouteUrl { get; set; }
        public bool IsImplemented { get; set; }
    }
}
