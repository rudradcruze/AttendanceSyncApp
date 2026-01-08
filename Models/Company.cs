using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttandanceSyncApp.Models
{
    [Table("Companies")]
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required] public int GroupId { get; set; } = 1;

        [Required]
        [StringLength(9)]
        public string WeekStrDay { get; set; }

        [Required]
        [StringLength(150)]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        public string Phone { get; set; }

        [StringLength(50)]
        public string Fax { get; set; }

        [Required]
        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(100)]
        public string IPAddress { get; set; }

        [Required]
        [StringLength(150)]
        public string ContactPerson { get; set; }

        [Required]
        public DateTime FinancialYearStart { get; set; }

        [Required]
        public DateTime FinancialYearTo { get; set; }

        [Required]
        [StringLength(50)]
        public string BaseCurrencyInWord { get; set; }

        [Required]
        [StringLength(20)]
        public string BaseCurrencyInSymbol { get; set; }

        [Required]
        public int ReportingLevelNo { get; set; }

        [Required]
        [StringLength(20)]
        public string DateTimeFormat { get; set; }

        [StringLength(50)]
        public string VoucherFormat { get; set; }

        [Required]
        [StringLength(10)]
        public string CompanyInitial { get; set; }

        public string CompanyLogo { get; set; }

        public DateTime? AccountingBookStartFrom { get; set; }

        public int? BaseCurrencyID { get; set; }

        [StringLength(200)]
        public string Slogan { get; set; }

        public string ReportSortingOrder { get; set; }
    }
}