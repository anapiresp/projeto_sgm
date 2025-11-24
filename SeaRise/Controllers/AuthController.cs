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

            // Login by username (not email)
            var filter = Builders<User>.Filter.Eq(u => u.username, model.Username);
            var user = await col.Find(filter).FirstOrDefaultAsync();

            if (user == null)
                return Unauthorized(new { message = "Nome de utilizador inválido" });

            if (user.password != model.Password)
                return Unauthorized(new { message = "Palavra-passe incorreta" });

            return Ok(new
            {
                message = "Login bem-sucedido",
                user = new { user.username, user.email, user.userType }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] SignupModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var collection = _mongo.GetCollection<User>("users");

            // Check if email or username already exists
            var emailFilter = Builders<User>.Filter.Eq(u => u.email, model.Email);
            var usernameFilter = Builders<User>.Filter.Eq(u => u.username, model.Username);
            var existing = await collection.Find(Builders<User>.Filter.Or(emailFilter, usernameFilter)).FirstOrDefaultAsync();
            if (existing != null)
            {
                return Conflict(new { message = "Email ou nome de utilizador já existente." });
            }

            var newUser = new User
            {
                username = model.Username,
                email = model.Email,
                password = model.Password,
                age = model.Age,
                userType = model.UserType,
                job = model.Job
            };

            await collection.InsertOneAsync(newUser);

            return CreatedAtAction(nameof(Register), new { email = newUser.email }, new { message = "Registo bem-sucedido", user = new { newUser.username, newUser.email, newUser.userType } });
        }
    }
}
