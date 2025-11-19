using SeaRise.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SeaRise.Models
{
    public class SignupModel
    {
        [Required(ErrorMessage = "Nome de utilizador é obrigatório.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; } = string.Empty;

        
        [Required(ErrorMessage = "Password é obrigatória.")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,50}$",
                ErrorMessage = "Password tem de ter mais de 8 caracteres, incluir uma letra maiúscula, uma letra minúscula, um número e um símbolo.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Idade é obrigatório.")]
        [Range(18, 120, ErrorMessage = "Tens de ter pelo menos 18 anos para te poderes registar.")]

        public int Age { get; set; }

        [Required(ErrorMessage = "Tem de escolher um tipo de utilizador: geral , trabalhador, empregador e admin.")]
        public string UserType { get; set; } = string.Empty;
    }
}