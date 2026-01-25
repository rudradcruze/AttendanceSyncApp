using System.ComponentModel.DataAnnotations;

namespace AttandanceSyncApp.Models.DTOs.AttandanceSync
{
    public class CompanyRequestCreateDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int ToolId { get; set; }
    }
}
