using System;

namespace AttandanceSyncApp.Models.DTOs.Admin
{
    public class DatabaseConfigDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string DatabaseIP { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUserId { get; set; }
        public string DatabasePassword { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DatabaseConfigCreateDto
    {
        public int CompanyId { get; set; }
        public string DatabaseIP { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUserId { get; set; }
        public string DatabasePassword { get; set; }
    }

    public class DatabaseConfigUpdateDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string DatabaseIP { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUserId { get; set; }
        public string DatabasePassword { get; set; }
    }
}
