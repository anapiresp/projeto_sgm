using SeaRise.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SeaRise.Models
{
    public class SignupModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        
        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,50}$",
                ErrorMessage = "Password must be more than 8 characters and include an uppercase letter, "
                                 + "a number, and a symbol.")]
        public string Password { get; set; } = string.Empty;
    }
}