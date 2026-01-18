using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AttandanceSyncApp.Models.Auth;

namespace AttandanceSyncApp.Models.Sync
{
    [Table("AttandanceSyncRequests")]
    public class AttandanceSyncRequest
    {
        [Key]
        public int Id { get; set; }

        // Who submitted the request
        [Required]
        public int UserId { get; set; }

        // Who the request is about
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int ToolId { get; set; }

        [Required]
        public int SessionId { get; set; }

        // External dynamic DB reference
        public int? ExternalSyncId { get; set; }

        public bool? IsSuccessful { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [ForeignKey("CompanyId")]
        public virtual SyncCompany Company { get; set; }

        [ForeignKey("ToolId")]
        public virtual Tool Tool { get; set; }

        [ForeignKey("SessionId")]
        public virtual LoginSession Session { get; set; }

        public virtual DatabaseConfiguration DatabaseConfiguration { get; set; }
    }
}
