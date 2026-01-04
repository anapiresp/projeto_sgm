using System.ComponentModel.DataAnnotations;

namespace SeaRise.Models
{
    public class ChangePasswordModel
    {
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password atual é obrigatória")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nova password é obrigatória")]
        [RegularExpression(@"^.{8,}$",
                ErrorMessage = "Password tem de ter mais de 8 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
