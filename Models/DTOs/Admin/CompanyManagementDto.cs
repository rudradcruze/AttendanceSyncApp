using System;

namespace AttandanceSyncApp.Models.DTOs.Admin
{
    public class CompanyManagementDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
