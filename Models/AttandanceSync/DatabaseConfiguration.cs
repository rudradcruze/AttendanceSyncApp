using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttandanceSyncApp.Models.AttandanceSync
{
    [Table("DatabaseConfigurations")]
    public class DatabaseConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(100)]
        public string DatabaseIP { get; set; }

        [Required]
        [StringLength(100)]
        public string DatabaseUserId { get; set; }

        [Required]
        [StringLength(500)]
        public string DatabasePassword { get; set; }

        [Required]
        [StringLength(150)]
        public string DatabaseName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("CompanyId")]
        public virtual SyncCompany Company { get; set; }
    }
}
