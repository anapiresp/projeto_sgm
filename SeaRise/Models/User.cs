using SeaRise.Interfaces;

namespace SeaRise.Services
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Age { get; set; }
        public string UserType { get; set; } = string.Empty;
    }
}