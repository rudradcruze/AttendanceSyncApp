using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttandanceSyncApp.Models.SalaryGarbge
{
    [Table("ServerIps")]
    public class ServerIp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string IpAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string DatabaseUser { get; set; }

        [Required]
        [StringLength(500)]
        public string DatabasePassword { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
