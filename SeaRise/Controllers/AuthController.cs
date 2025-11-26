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
    }
}
