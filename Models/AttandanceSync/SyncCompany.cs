using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttandanceSyncApp.Models.AttandanceSync
{
    [Table("Companies")]
    public class SyncCompany
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<AttandanceSyncRequest> SyncRequests { get; set; }
        public virtual ICollection<CompanyRequest> CompanyRequests { get; set; }

        public SyncCompany()
        {
            SyncRequests = new HashSet<AttandanceSyncRequest>();
            CompanyRequests = new HashSet<CompanyRequest>();
        }
    }
}
