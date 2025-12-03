using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SeaRise.Models;
using SeaRise.Services.Database;

namespace SeaRise.Controllers
{
    [ApiController]
    [Route("api/{category}")]
    public class JobsController : ControllerBase
    {
        private readonly MongoService _mongo;

        public JobsController(MongoService mongo)
        {
            _mongo = mongo;
        }

        /// Lista jobs armazenados na base de dados. Filtra por categoria (case-insensitive) e suporta paginação.
        [HttpGet]
        public async Task<ActionResult> Get([FromRoute] string category, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var collection = _mongo.GetCollection<Job>("job");

            // Case-insensitive exact match on category provided by the route
            var filter = Builders<Job>.Filter.Regex(j => j.Category, new BsonRegularExpression($"^{Regex.Escape(category)}$", "i"));

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            // total matching documents for the provided filter (useful for front-end pagination)
            var total = await collection.CountDocumentsAsync(filter);

            var skip = (page - 1) * pageSize;
            var items = await collection.Find(filter).Skip(skip).Limit(pageSize).ToListAsync();

            return Ok(new { items, total });
        }

        /// Obtém detalhes de um job por id.
        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetById([FromRoute] string category, string id)
        {
            var collection = _mongo.GetCollection<Job>("job");
            var fb = Builders<Job>.Filter;
            var filter = fb.Eq(j => j.Id, id) & fb.Regex(j => j.Category, new BsonRegularExpression($"^{Regex.Escape(category)}$", "i"));
            var job = await collection.Find(filter).FirstOrDefaultAsync();
            if (job == null) return NotFound();
            return Ok(job);
        }
    }
}
