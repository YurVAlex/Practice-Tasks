using System.ComponentModel.DataAnnotations;

namespace ASP_Empty_WebApplication_01.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public required string Email { get; set; }

        [Required]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters long.")]
        public required string Password { get; set; }
    }
}
