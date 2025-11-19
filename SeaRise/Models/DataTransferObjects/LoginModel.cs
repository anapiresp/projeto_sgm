using SeaRise.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SeaRise.Models
{
    public class LoginModel
    {
        [Required][EmailAddress(ErrorMessage = "Username é obrigatório.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password é obrigatório.")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,50}$",
                ErrorMessage = "Password tem de ter entre 8 a 50 caracteres, incluir pelo menos uma letra maiúscula, uma letra minúscula, um número e um símbolo.")]
        public string? Password { get; set; }
    }
}