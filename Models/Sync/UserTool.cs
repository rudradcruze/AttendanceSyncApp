using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AttandanceSyncApp.Models.Auth;

namespace AttandanceSyncApp.Models.Sync
{
    [Table("UserTools")]
    public class UserTool
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ToolId { get; set; }

        [Required]
        public int AssignedBy { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ToolId")]
        public virtual Tool Tool { get; set; }

        [ForeignKey("AssignedBy")]
        public virtual User AssignedByUser { get; set; }
    }
}
