using System.ComponentModel.DataAnnotations;

namespace SeaRise.Models
{
    public class LoginModel
    {
        [Required (ErrorMessage = "Username ou email é obrigatório.")]
        public string? UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Password é obrigatório.")]
        public string? Password { get; set; }
    }
}