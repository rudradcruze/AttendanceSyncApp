using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttandanceSyncApp.Models.AttandanceSync
{
    [Table("AttandanceSynchronizations")]
    public class AttandanceSynchronization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        public int CompanyId { get; set; }

        public string Status { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
    }
}
