using System;

namespace AttandanceSyncApp.Models.DTOs.SalaryGarbge
{
    public class ServerIpDto
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ServerIpCreateDto
    {
        public string IpAddress { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ServerIpUpdateDto
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
