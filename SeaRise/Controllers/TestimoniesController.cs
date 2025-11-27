using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SeaRise.Models;
using SeaRise.Models.DataTransferObjects;
using SeaRise.Services.Database;

namespace SeaRise.Controllers
{
    [ApiController]
    public class TestimoniesController : ControllerBase
    {
        private readonly MongoService _mongo;

        public TestimoniesController(MongoService mongo)
        {
            _mongo = mongo;
        }

        /// Lista testimonies para um job específico. Por defeito só retorna testimonies aprovados.
        [HttpGet("api/{category}/{jobId}/testimonies")]
        public async Task<ActionResult> GetForJob([FromRoute] string category, [FromRoute] string jobId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool approved = true)
        {
            // verifica se o job existe e pertence à categoria pedida
            var jobsColl = _mongo.GetCollection<Job>("job");
            var jobFilter = Builders<Job>.Filter.Eq(j => j.Id, jobId);
            var job = await jobsColl.Find(jobFilter).FirstOrDefaultAsync();
            if (job == null) return NotFound();
            if (!string.Equals(job.Category, category, StringComparison.OrdinalIgnoreCase)) return NotFound();

            var collection = _mongo.GetCollection<JobTestimony>("job_testimony");

            // Remove testimonies previamente rejeitados para este job antes de listar
            var rejectedFilter = Builders<JobTestimony>.Filter.Eq(t => t.JobId, jobId) & Builders<JobTestimony>.Filter.Eq(t => t.WasRejected, true);
            await collection.DeleteManyAsync(rejectedFilter);

            var fb = Builders<JobTestimony>.Filter;
            var filter = fb.Eq(t => t.JobId, jobId);
            if (approved)
            {
                filter = filter & fb.Eq(t => t.IsApproved, true);
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var total = await collection.CountDocumentsAsync(filter);
            var skip = (page - 1) * pageSize;
            var items = await collection.Find(filter).SortByDescending(t => t.CreatedAt).Skip(skip).Limit(pageSize).ToListAsync();

            return Ok(new { items, total });
        }

        /// Cria um testimony para um job, mas apenas pessoas que exercem  profissão podem deixar testemunhos. O testimony é criado com `approved=false` por defeito.
        [HttpPost("api/{category}/{jobId}/testimonies")]
        public async Task<ActionResult> CreateForJob([FromRoute] string category, [FromRoute] string jobId, [FromBody] TestimonyCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // verifica se o job existe e pertence à categoria pedida
            var jobsColl = _mongo.GetCollection<Job>("job");
            var jobFilter = Builders<Job>.Filter.Eq(j => j.Id, jobId);
            var job = await jobsColl.Find(jobFilter).FirstOrDefaultAsync();
            if (job == null) return NotFound();
            if (!string.Equals(job.Category, category, StringComparison.OrdinalIgnoreCase)) return BadRequest("Job does not belong to the specified category.");

            // verifica se o utilizador existe, é do tipo 'trabalhador' e está associado a este job
            var usersColl = _mongo.GetCollection<BsonDocument>("users");
            BsonDocument? userDoc = null;
            if (ObjectId.TryParse(dto.UserId, out var userObjId))
            {
                userDoc = await usersColl.Find(Builders<BsonDocument>.Filter.Eq("_id", userObjId)).FirstOrDefaultAsync();
            }
            if (userDoc == null)
            {
                userDoc = await usersColl.Find(Builders<BsonDocument>.Filter.Eq("Id", dto.UserId)).FirstOrDefaultAsync();
            }
            if (userDoc == null) return BadRequest("Utilizador não encontrado.");

            string? userType = null;
            if (userDoc.Contains("UserType")) userType = userDoc.GetValue("UserType").ToString();
            else if (userDoc.Contains("userType")) userType = userDoc.GetValue("userType").ToString();

            if (string.IsNullOrWhiteSpace(userType) || !string.Equals(userType, "trabalhador", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid("Apenas utilizadores do tipo 'trabalhador' podem criar testemunhos para empregos.");
            }

            string? userJob = null;
            if (userDoc.Contains("Job")) userJob = userDoc.GetValue("Job").ToString();
            else if (userDoc.Contains("job")) userJob = userDoc.GetValue("job").ToString();

            var jobTitle = job?.Title ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userJob) || !string.Equals(userJob.Trim(), jobTitle.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Forbid("Utilizador não está associado a este emprego e não pode criar um testemunho para ele.");
            }

            var collection = _mongo.GetCollection<JobTestimony>("job_testimony");
            var testimony = new JobTestimony
            {
                JobId = jobId,
                UserId = dto.UserId,
                Content = dto.Content,
                MediaUrl = dto.MediaUrl ?? string.Empty,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            await collection.InsertOneAsync(testimony);

            return CreatedAtAction(nameof(GetById), new { category = category, jobId = jobId, id = testimony.Id }, testimony);
        }

        /// Aprova um testimony (marca is_approved = true). Validação: job existe e pertence à categoria, testimony pertence ao job.
        [HttpPost("api/{category}/{jobId}/testimonies/{id}/approve")]
        public async Task<ActionResult> Approve(string category, string jobId, string id)
        {
            var jobsColl = _mongo.GetCollection<Job>("job");
            var jobFilter = Builders<Job>.Filter.Eq(j => j.Id, jobId);
            var job = await jobsColl.Find(jobFilter).FirstOrDefaultAsync();
            if (job == null) return NotFound();
            if (!string.Equals(job.Category, category, StringComparison.OrdinalIgnoreCase)) return NotFound();

            // Verifica se o request vem de um admin (header X-User-Id)
            var usersColl = _mongo.GetCollection<BsonDocument>("users");
            if (!Request.Headers.ContainsKey("X-User-Id")) return Forbid("Cabeçalho 'X-User-Id' necessário para moderação.");
            var adminId = Request.Headers["X-User-Id"].ToString();
            BsonDocument? adminDoc = null;
            if (ObjectId.TryParse(adminId, out var adminObjId))
            {
                adminDoc = await usersColl.Find(Builders<BsonDocument>.Filter.Eq("_id", adminObjId)).FirstOrDefaultAsync();
            }
            if (adminDoc == null)
            {
                adminDoc = await usersColl.Find(Builders<BsonDocument>.Filter.Eq("Id", adminId)).FirstOrDefaultAsync();
            }
            if (adminDoc == null) return Forbid("Administrador não encontrado.");
            string? adminType = null;
            if (adminDoc.Contains("UserType")) adminType = adminDoc.GetValue("UserType").ToString();
            else if (adminDoc.Contains("user_type")) adminType = adminDoc.GetValue("user_type").ToString();
            if (string.IsNullOrWhiteSpace(adminType) || !string.Equals(adminType, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid("Apenas administradores podem aprovar testemunhos.");
            }

            var coll = _mongo.GetCollection<JobTestimony>("job_testimony");
            var filter = Builders<JobTestimony>.Filter.Eq(t => t.Id, id) & Builders<JobTestimony>.Filter.Eq(t => t.JobId, jobId);
            var update = Builders<JobTestimony>.Update.Set(t => t.IsApproved, true).Set(t => t.WasRejected, false);
            var res = await coll.UpdateOneAsync(filter, update);
            if (res.MatchedCount == 0) return NotFound();
            return NoContent();
        }

        /// Rejeita um testimony (is_approved = rejected & was_rejected = true). Validação: job existe e pertence à categoria, testimony pertence ao job.
        [HttpPost("api/{category}/{jobId}/testimonies/{id}/reject")]
        public async Task<ActionResult> Reject(string category, string jobId, string id)
        {
            var jobsColl = _mongo.GetCollection<Job>("job");
            var jobFilter = Builders<Job>.Filter.Eq(j => j.Id, jobId);
            var job = await jobsColl.Find(jobFilter).FirstOrDefaultAsync();
            if (job == null) return NotFound();
            if (!string.Equals(job.Category, category, StringComparison.OrdinalIgnoreCase)) return NotFound();

            // Verifica se o request vem de um admin (header X-User-Id)
            var usersColl = _mongo.GetCollection<BsonDocument>("users");
            if (!Request.Headers.ContainsKey("X-User-Id")) return Forbid("Cabeçalho 'X-User-Id' necessário para moderação.");
            var adminId = Request.Headers["X-User-Id"].ToString();
            BsonDocument? adminDoc = null;
            if (ObjectId.TryParse(adminId, out var adminObjId))
            {
                adminDoc = await usersColl.Find(Builders<BsonDocument>.Filter.Eq("_id", adminObjId)).FirstOrDefaultAsync();
            }
            if (adminDoc == null)
            {
                adminDoc = await usersColl.Find(Builders<BsonDocument>.Filter.Eq("Id", adminId)).FirstOrDefaultAsync();
            }
            if (adminDoc == null) return Forbid("Administrador não encontrado.");
            string? adminType = null;
            if (adminDoc.Contains("UserType")) adminType = adminDoc.GetValue("UserType").ToString();
            else if (adminDoc.Contains("userType")) adminType = adminDoc.GetValue("userType").ToString();
            if (string.IsNullOrWhiteSpace(adminType) || !string.Equals(adminType, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid("Apenas administradores podem rejeitar testemunhos.");
            }

            var coll = _mongo.GetCollection<JobTestimony>("job_testimony");
            var filter = Builders<JobTestimony>.Filter.Eq(t => t.Id, id) & Builders<JobTestimony>.Filter.Eq(t => t.JobId, jobId);
            var update = Builders<JobTestimony>.Update.Set(t => t.IsApproved, false);
            update = update.Set(t => t.WasRejected, true);
            var res = await coll.UpdateOneAsync(filter, update);
            if (res.MatchedCount == 0) return NotFound();
            return NoContent();
        }

        /// Detalhe de um testimony por id.
        [HttpGet("api/{category}/{jobId}/testimonies/{id}")]
        public async Task<ActionResult<JobTestimony>> GetById(string category, string jobId, string id)
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
