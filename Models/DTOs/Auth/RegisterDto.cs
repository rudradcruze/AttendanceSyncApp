using System.ComponentModel.DataAnnotations;

namespace AttandanceSyncApp.Models.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 150 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
