using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SeaRise.Models;
using SeaRise.Models.DataTransferObjects;
using SeaRise.Services.Database;

namespace SeaRise.Controllers.Comments
{
    [ApiController]
    [Route("api/{category}/{jobId}/testimonies/{testimonyId}")]
    public class ComJobController : ControllerBase
    {
        private readonly MongoService _mongo;
        public ComJobController(MongoService mongo)
        {
            _mongo = mongo;
        }

        // GET testimony detail (non-conflicting path)
        // Changed from [HttpGet] to [HttpGet("info")] to avoid route collision with TestimoniesController.GetById
        [HttpGet("info")]
        public async Task<IActionResult> GetTestimony([FromRoute] string jobId, [FromRoute] string testimonyId)
        {
            var collection = _mongo.GetCollection<JobTestimony>("job_testimony");

            var filter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId) &
                         Builders<JobTestimony>.Filter.Eq(t => t.JobId, jobId);

            var testimony = await collection.Find(filter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();
            else if (testimony.IsApproved == false || testimony.WasRejected == true)
                return Forbid();

            return Ok(testimony);
        }

        // GET comments list
        [HttpGet("comments")]
        public async Task<IActionResult> GetComments([FromRoute] string category, [FromRoute] string jobId, [FromRoute] string testimonyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool approved = true)
        {
            // valida testimony/job/category
            var jobsColl = _mongo.GetCollection<Job>("job");
            var jobFilter = Builders<Job>.Filter.Eq(j => j.Id, jobId);
            var job = await jobsColl.Find(jobFilter).FirstOrDefaultAsync();
            if (job == null) return NotFound();
            if (!string.Equals(job.Category, category, StringComparison.OrdinalIgnoreCase)) return NotFound();

            var testimoniesColl = _mongo.GetCollection<JobTestimony>("job_testimony");
            var tFilter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId) & Builders<JobTestimony>.Filter.Eq(t => t.JobId, jobId);
            var testimony = await testimoniesColl.Find(tFilter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();
            if (testimony.IsApproved == false || testimony.WasRejected == true) return Forbid();

            var coll = _mongo.GetCollection<JobComment>("job_testimony_comment");

            // delete rejected comments for this testimony
            var rejectedFilter = Builders<JobComment>.Filter.Eq(c => c.JobId, testimonyId) & Builders<JobComment>.Filter.Eq(c => c.WasRejected, true);
            await coll.DeleteManyAsync(rejectedFilter);

            var fb = Builders<JobComment>.Filter;
            var filter = fb.Eq(c => c.JobId, testimonyId);
            if (approved)
            {
                filter = filter & fb.Eq(c => c.IsApproved, true);
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var total = await coll.CountDocumentsAsync(filter);
            var skip = (page - 1) * pageSize;
            var items = await coll.Find(filter).SortByDescending(c => c.CreatedAt).Skip(skip).Limit(pageSize).ToListAsync();

            return Ok(new { items, total });
        }

        // GET comment detail
        [HttpGet("comments/{id}")]
        public async Task<IActionResult> GetCommentById([FromRoute] string category, [FromRoute] string jobId, [FromRoute] string testimonyId, string id)
        {
            var testimoniesColl = _mongo.GetCollection<JobTestimony>("job_testimony");
            var tFilter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId) & Builders<JobTestimony>.Filter.Eq(t => t.JobId, jobId);
            var testimony = await testimoniesColl.Find(tFilter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();

            var coll = _mongo.GetCollection<JobComment>("job_testimony_comment");
            var filter = Builders<JobComment>.Filter.Eq(c => c.Id, id) & Builders<JobComment>.Filter.Eq(c => c.JobId, testimonyId);
            var comment = await coll.Find(filter).FirstOrDefaultAsync();
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        // POST create comment
        [HttpPost("comments")]
        public async Task<IActionResult> CreateComment([FromRoute] string category, [FromRoute] string jobId, [FromRoute] string testimonyId, [FromBody] TestimonyCommentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var testimoniesColl = _mongo.GetCollection<JobTestimony>("job_testimony");
            var tFilter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId) & Builders<JobTestimony>.Filter.Eq(t => t.JobId, jobId);
            var testimony = await testimoniesColl.Find(tFilter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();
            if (testimony.IsApproved == false || testimony.WasRejected == true) return Forbid();

            // verify user exists
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
            if (userDoc == null) return NotFound("Utilizador não encontrado.");

            var coll = _mongo.GetCollection<JobComment>("job_testimony_comment");
            var comment = new JobComment
            {
                JobId = testimonyId,
                UserId = dto.UserId ?? string.Empty,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                IsApproved = false,
                WasRejected = false
            };

            await coll.InsertOneAsync(comment);
            return CreatedAtAction(nameof(GetCommentById), new { category = category, jobId = jobId, testimonyId = testimonyId, id = comment.Id }, comment);
        }

        // POST aprovar comentário (admin apenas)
        [HttpPost("comments/{id}/approve")]
        public async Task<IActionResult> ApproveComment([FromRoute] string category, [FromRoute] string jobId, [FromRoute] string testimonyId, string id)
        {
            // validar job/testimony
            var jobsColl = _mongo.GetCollection<Job>("job");
            var jobFilter = Builders<Job>.Filter.Eq(j => j.Id, jobId);
            var job = await jobsColl.Find(jobFilter).FirstOrDefaultAsync();
            if (job == null) return NotFound();
            if (!string.Equals(job.Category, category, StringComparison.OrdinalIgnoreCase)) return NotFound();

            var testimoniesColl = _mongo.GetCollection<JobTestimony>("job_testimony");
            var tFilter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId) & Builders<JobTestimony>.Filter.Eq(t => t.JobId, jobId);
            var testimony = await testimoniesColl.Find(tFilter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();

            //verifica se o comentário foi rejeitado anteriormente
            var commentsColl = _mongo.GetCollection<JobComment>("job_testimony_comment");
            var commentFilter = Builders<JobComment>.Filter.Eq(c => c.Id, id) & Builders<JobComment>.Filter.Eq(c => c.JobId, testimonyId);
            var comment = await commentsColl.Find(commentFilter).FirstOrDefaultAsync();
            if (comment == null) return NotFound();
            if (comment.WasRejected == true)
            {
                return BadRequest("Não é possível aprovar um comentário já rejeitado.");
            }

           // Verifica se o request vem de um admin
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
            if (adminDoc.Contains("User_type")) adminType = adminDoc.GetValue("User_type").ToString();
            else if (adminDoc.Contains("user_type")) adminType = adminDoc.GetValue("user_type").ToString();
            if (string.IsNullOrWhiteSpace(adminType) || !string.Equals(adminType, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid("Apenas administradores podem aprovar comentários.");
            }

            var coll = _mongo.GetCollection<JobComment>("job_testimony_comment");
            var filter = Builders<JobComment>.Filter.Eq(c => c.Id, id) & Builders<JobComment>.Filter.Eq(c => c.JobId, testimonyId);
            var update = Builders<JobComment>.Update.Set(c => c.IsApproved, true).Set(c => c.WasRejected, false);
            var res = await coll.UpdateOneAsync(filter, update);
            if (res.MatchedCount == 0) return NotFound();
            return NoContent();
        }

        // POST rejeitar comentário (admin apenas)
        [HttpPost("comments/{id}/reject")]
        public async Task<IActionResult> RejectComment([FromRoute] string category, [FromRoute] string jobId, [FromRoute] string testimonyId, string id)
        {
            // validar job/testimony
            var jobsColl = _mongo.GetCollection<Job>("job");
            var jobFilter = Builders<Job>.Filter.Eq(j => j.Id, jobId);
            var job = await jobsColl.Find(jobFilter).FirstOrDefaultAsync();
            if (job == null) return NotFound();
            if (!string.Equals(job.Category, category, StringComparison.OrdinalIgnoreCase)) return NotFound();

            var testimoniesColl = _mongo.GetCollection<JobTestimony>("job_testimony");
            var tFilter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId) & Builders<JobTestimony>.Filter.Eq(t => t.JobId, jobId);
            var testimony = await testimoniesColl.Find(tFilter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();

            //verifica se o comentário já foi aprovado
            var commentsColl = _mongo.GetCollection<JobComment>("job_testimony_comment");
            var commentFilter = Builders<JobComment>.Filter.Eq(c => c.Id, id) & Builders<JobComment>.Filter.Eq(c => c.JobId, testimonyId);
            var comment = await commentsColl.Find(commentFilter).FirstOrDefaultAsync();
            if (comment == null) return NotFound();
            if (comment.IsApproved == true)
            {
                return BadRequest("Não é possível rejeitar um comentário já aprovado.");
            }

            // Verifica se o request vem de um admin
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
            if (adminDoc.Contains("User_type")) adminType = adminDoc.GetValue("User_type").ToString();
            else if (adminDoc.Contains("user_type")) adminType = adminDoc.GetValue("user_type").ToString();
            if (string.IsNullOrWhiteSpace(adminType) || !string.Equals(adminType, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid("Apenas administradores podem rejeitar comentários.");
            }

            var coll = _mongo.GetCollection<JobComment>("job_testimony_comment");
            var filter = Builders<JobComment>.Filter.Eq(c => c.Id, id) & Builders<JobComment>.Filter.Eq(c => c.JobId, testimonyId);
            var update = Builders<JobComment>.Update.Set(c => c.IsApproved, false).Set(c => c.WasRejected, true);
            var res = await coll.UpdateOneAsync(filter, update);
            if (res.MatchedCount == 0) return NotFound();
            return NoContent();
        }

        // DELETE comentário (autor)
        [HttpDelete("comments/{id}")]
        public async Task<IActionResult> DeleteComment([FromRoute] string category, [FromRoute] string jobId, [FromRoute] string testimonyId, string id)
        {
            var testimoniesColl = _mongo.GetCollection<JobTestimony>("job_testimony");
            var tFilter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId) & Builders<JobTestimony>.Filter.Eq(t => t.JobId, jobId);
            var testimony = await testimoniesColl.Find(tFilter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();

            var coll = _mongo.GetCollection<JobComment>("job_testimony_comment");
            var filter = Builders<JobComment>.Filter.Eq(c => c.Id, id) & Builders<JobComment>.Filter.Eq(c => c.JobId, testimonyId);
            var comment = await coll.Find(filter).FirstOrDefaultAsync();
            if (comment == null) return NotFound();

            if (!Request.Headers.ContainsKey("X-User-Id")) return Forbid("Cabeçalho 'X-User-Id' necessário para esta operação.");
            var requesterId = Request.Headers["X-User-Id"].ToString();

            var usersColl = _mongo.GetCollection<BsonDocument>("users");
            BsonDocument? requesterDoc = null;
            if (ObjectId.TryParse(requesterId, out var reqObjId))
            {
                requesterDoc = await usersColl.Find(Builders<BsonDocument>.Filter.Eq("_id", reqObjId)).FirstOrDefaultAsync();
            }
            if (requesterDoc == null)
            {
                requesterDoc = await usersColl.Find(Builders<BsonDocument>.Filter.Eq("Id", requesterId)).FirstOrDefaultAsync();
            }
            if (requesterDoc == null) return Forbid("Utilizador não encontrado.");

            var isAuthor = false;
            var storedUserId = comment.UserId ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(storedUserId) && !string.IsNullOrWhiteSpace(requesterId))
            {
                if (ObjectId.TryParse(storedUserId, out var storedObj) && ObjectId.TryParse(requesterId, out var reqObj))
                {
                    isAuthor = storedObj == reqObj;
                }
                else
                {
                    isAuthor = string.Equals(storedUserId.Trim(), requesterId.Trim(), StringComparison.OrdinalIgnoreCase);
                }
            }

            if (!isAuthor) return Forbid("Só o autor pode apagar este comentário.");

            var del = await coll.DeleteOneAsync(filter);
            if (del.DeletedCount == 0) return NotFound();
            return NoContent();
        }
    }
}
