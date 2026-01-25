using System;

namespace AttandanceSyncApp.Models.DTOs.SalaryGarbge
{
    public class DatabaseAccessDto
    {
        public int Id { get; set; }
        public int ServerIpId { get; set; }
        public string DatabaseName { get; set; }
        public bool HasAccess { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DatabaseAccessListDto
    {
        public string DatabaseName { get; set; }
        public bool HasAccess { get; set; }
        public bool ExistsInAccessTable { get; set; } // For marking new databases
        public int? DatabaseAccessId { get; set; } // Null if new database
    }

    public class UpdateDatabaseAccessDto
    {
        public int ServerIpId { get; set; }
        public string DatabaseName { get; set; }
        public bool HasAccess { get; set; }
    }
}
