using System.ComponentModel.DataAnnotations;

namespace ChathamHouse.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string ClubName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[\/\.,\+]).+$",
            ErrorMessage = "Password must include uppercase, lowercase, number, and /.,+ symbols")]
        public string Password { get; set; }

        [Required]
        public string Location { get; set; }
        
    }
}
