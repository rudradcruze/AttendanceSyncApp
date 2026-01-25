using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttandanceSyncApp.Models.SalaryGarbge
{
    [Table("DatabaseAccess")]
    public class DatabaseAccess
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ServerIpId { get; set; }

        [Required]
        [StringLength(255)]
        public string DatabaseName { get; set; }

        public bool HasAccess { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("ServerIpId")]
        public virtual ServerIp ServerIp { get; set; }
    }
}
