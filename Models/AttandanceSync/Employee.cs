using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttandanceSyncApp.Models.AttandanceSync
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<AttandanceSyncRequest> SyncRequests { get; set; }
        public virtual ICollection<CompanyRequest> CompanyRequests { get; set; }

        public Employee()
        {
            SyncRequests = new HashSet<AttandanceSyncRequest>();
            CompanyRequests = new HashSet<CompanyRequest>();
        }
    }
}
