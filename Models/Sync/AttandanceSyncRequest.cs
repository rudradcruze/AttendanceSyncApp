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

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int ToolId { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(5)]
        public string Status { get; set; } = "NR";

        [Required]
        public int SessionId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("CompanyId")]
        public virtual SyncCompany Company { get; set; }

        [ForeignKey("ToolId")]
        public virtual Tool Tool { get; set; }

        [ForeignKey("SessionId")]
        public virtual LoginSession Session { get; set; }

        public virtual DatabaseConfiguration DatabaseConfiguration { get; set; }
    }
}
