using System;

namespace AttandanceSyncApp.Models.DTOs.Admin
{
    public class SessionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string Device { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
