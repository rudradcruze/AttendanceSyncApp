using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttandanceSyncApp.Models.Sync
{
    [Table("Tools")]
    public class Tool
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsUnderDevelopment { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<AttandanceSyncRequest> SyncRequests { get; set; }
        public virtual ICollection<CompanyRequest> CompanyRequests { get; set; }
        public virtual ICollection<UserTool> UserTools { get; set; }

        public Tool()
        {
            SyncRequests = new HashSet<AttandanceSyncRequest>();
            CompanyRequests = new HashSet<CompanyRequest>();
            UserTools = new HashSet<UserTool>();
        }
    }
}
