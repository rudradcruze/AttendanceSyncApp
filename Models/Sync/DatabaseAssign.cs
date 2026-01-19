using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AttandanceSyncApp.Models.Auth;

namespace AttandanceSyncApp.Models.Sync
{
    /// <summary>
    /// Represents a database assignment for a company request.
    /// Links a CompanyRequest to a DatabaseConfiguration with audit information.
    /// </summary>
    [Table("DatabaseAssignments")]
    public class DatabaseAssign
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyRequestId { get; set; }

        [Required]
        public int AssignedBy { get; set; }

        [Required]
        public int DatabaseConfigurationId { get; set; }

        [Required]
        public DateTime AssignedAt { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("CompanyRequestId")]
        public virtual CompanyRequest CompanyRequest { get; set; }

        [ForeignKey("AssignedBy")]
        public virtual User AssignedByUser { get; set; }

        [ForeignKey("DatabaseConfigurationId")]
        public virtual DatabaseConfiguration DatabaseConfiguration { get; set; }
    }
}
