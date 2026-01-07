using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSyncApp.Models
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

        [Required]
        [StringLength(10)]
        public string Status { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
    }
}