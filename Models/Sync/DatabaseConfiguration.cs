using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AttandanceSyncApp.Models.Auth;

namespace AttandanceSyncApp.Models.Sync
{
    [Table("DatabaseConfigurations")]
    public class DatabaseConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RequestId { get; set; }

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

        [Required]
        public int AssignedBy { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("RequestId")]
        public virtual AttandanceSyncRequest Request { get; set; }

        [ForeignKey("AssignedBy")]
        public virtual User AssignedByUser { get; set; }
    }
}
