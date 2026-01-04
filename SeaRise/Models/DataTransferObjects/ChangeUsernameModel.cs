using System.ComponentModel.DataAnnotations;

namespace SeaRise.Models
{
    public class ChangeUsernameModel
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Novo nome de utilizador é obrigatório")]
        public string NewName { get; set; } = string.Empty;
    }
}
