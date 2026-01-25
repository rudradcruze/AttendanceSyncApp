using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Models.Auth
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "USER";

        [StringLength(255)]
        public string GoogleId { get; set; }

        [StringLength(500)]
        public string ProfilePicture { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<LoginSession> LoginSessions { get; set; }
        public virtual ICollection<AttandanceSyncRequest> SyncRequests { get; set; }
        public virtual ICollection<CompanyRequest> CompanyRequests { get; set; }
        public virtual ICollection<DatabaseConfiguration> AssignedConfigurations { get; set; }
        public virtual ICollection<UserTool> UserTools { get; set; }

        public User()
        {
            LoginSessions = new HashSet<LoginSession>();
            SyncRequests = new HashSet<AttandanceSyncRequest>();
            CompanyRequests = new HashSet<CompanyRequest>();
            AssignedConfigurations = new HashSet<DatabaseConfiguration>();
            UserTools = new HashSet<UserTool>();
        }
    }
}
