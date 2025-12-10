using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SeaRise.Models;
using SeaRise.Models.DataTransferObjects;
using SeaRise.Services.Database;

namespace SeaRise.Controllers
{
    [ApiController]
    [Route("api/{category}/{jobId}/testimonies")]
    public class TestimoniesController : ControllerBase
    {
        private readonly MongoService _mongo;

        public TestimoniesController(MongoService mongo)
        {
            _mongo = mongo;
        }

        /// Lista testimonies para um job específico. Por defeito só retorna testimonies aprovados.
        [HttpGet]
        public async Task<ActionResult> GetForJob([FromRoute] string category, [FromRoute] string jobId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool approved = true)
        {
            // verifica se o job existe e pertence à categoria pedida
            var jobsColl = _mongo.GetCollection<Job>("job");
            var jobFilter = Builders<Job>.Filter.Eq(j => j.Id, jobId);
            var job = await jobsColl.Find(jobFilter).FirstOrDefaultAsync();
            if (job == null) return NotFound();
            if (!string.Equals(job.Category, category, StringComparison.OrdinalIgnoreCase)) return NotFound();

            var collection = _mongo.GetCollection<JobTestimony>("job_testimony");

            var fb = Builders<JobTestimony>.Filter;
            var filter = fb.Eq(t => t.JobId, jobId);

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var total = await collection.CountDocumentsAsync(filter);
            var skip = (page - 1) * pageSize;
            var items = await collection.Find(filter)
                                        .Skip(skip)
                                        .Limit(pageSize)
                                        .ToListAsync();

            return Ok(new { items, total });
        }

        /// Detalhe de um testimony por id.
        [HttpGet("{id}", Name = "GetTestimonyById")]
        public async Task<ActionResult<JobTestimony>> GetById([FromRoute] string category, [FromRoute] string jobId, [FromRoute] string id)
        {
            // verifica se o job existe e pertence à categoria pedida
            var jobsColl = _mongo.GetCollection<Job>("job");
            var jobFilter = Builders<Job>.Filter.Eq(j => j.Id, jobId);
            var job = await jobsColl.Find(jobFilter).FirstOrDefaultAsync();
            if (job == null) return NotFound();
            if (!string.Equals(job.Category, category, StringComparison.OrdinalIgnoreCase)) return NotFound();
            if (job.Id != jobId) return NotFound();

            var collection = _mongo.GetCollection<JobTestimony>("job_testimony");
            var filter = Builders<JobTestimony>.Filter.Eq(t => t.Id, id);
            var testimony = await collection.Find(filter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();
            return Ok(testimony);
        }
    }
}
