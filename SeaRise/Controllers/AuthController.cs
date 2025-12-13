using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SeaRise.Models;
using SeaRise.Services;
using SeaRise.Services.Database;

namespace SeaRise.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly MongoService _mongo;

        public AuthController(MongoService mongo)
        {
            _mongo = mongo;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var col = _mongo.GetCollection<User>("users");

            // Login by username or email
            var filter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Eq(u => u.Username, model.UsernameOrEmail),
                Builders<User>.Filter.Eq(u => u.Email, model.UsernameOrEmail)
            );
            var user = await col.Find(filter).FirstOrDefaultAsync();

            if (user == null)
                return Unauthorized(new { message = "Nome de utilizador ou email inválido" });

            if (user.Password != model.Password)
                return Unauthorized(new { message = "Palavra-passe incorreta" });

            return Ok(new
            {
                message = "Login bem-sucedido",
                user = new { user.Username, user.Email, user.UserType }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] SignupModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var collection = _mongo.GetCollection<User>("users");

            // Check if email or username already exists
            var emailFilter = Builders<User>.Filter.Eq(u => u.Email, model.Email);
            var usernameFilter = Builders<User>.Filter.Eq(u => u.Username, model.Username);
            var existing = await collection.Find(Builders<User>.Filter.Or(emailFilter, usernameFilter)).FirstOrDefaultAsync();
            if (existing != null)
            {
                return Conflict(new { message = "Email ou nome de utilizador já existente." });
            }

            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password,
                Age = model.Age,
                UserType = model.UserType,
                Job = model.Job
            };

            await collection.InsertOneAsync(newUser);

            return CreatedAtAction(nameof(Register), new { email = newUser.Email }, new { message = "Registo bem-sucedido", user = new { newUser.Username, newUser.Email, newUser.UserType } });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email é obrigatório" });

            var collection = _mongo.GetCollection<User>("users");
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var user = await collection.Find(filter).FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Utilizador não encontrado" });

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                age = user.Age,
                userType = user.UserType,
                job = user.Job
            });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var collection = _mongo.GetCollection<User>("users");

            // Find user by email
            var filter = Builders<User>.Filter.Eq(u => u.Email, model.Email);
            var user = await collection.Find(filter).FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Utilizador não encontrado" });

            // Verify current password
            if (user.Password != model.CurrentPassword)
                return Unauthorized(new { message = "Password atual incorreta" });

            // Check if new password is different
            if (model.CurrentPassword == model.NewPassword)
                return BadRequest(new { message = "A nova password tem de ser diferente da atual" });

            // Update password
            var update = Builders<User>.Update.Set(u => u.Password, model.NewPassword);
            await collection.UpdateOneAsync(filter, update);

            return Ok(new { message = "Password alterada com sucesso" });
        }
    }
}
