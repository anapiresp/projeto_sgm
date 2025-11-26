using System.ComponentModel.DataAnnotations;

namespace SeaRise.Models
{
    public class LoginModel
    {
        [Required (ErrorMessage = "Username ou email é obrigatório.")]
        public string? UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Password é obrigatório.")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,50}$",
                ErrorMessage = "Password tem de ter entre 8 a 50 caracteres, incluir pelo menos uma letra maiúscula, uma letra minúscula, um número e um símbolo.")]
        public string? Password { get; set; }
    }
}