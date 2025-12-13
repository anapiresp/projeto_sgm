using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SeaRise.Models;
using SeaRise.Models.DataTransferObjects;
using SeaRise.Services.Database;

namespace SeaRise.Controllers.Comments
{
    [ApiController]
    [Route("api/{job}/testimonies/{testimonyId}")]
    public class ComJobController : ControllerBase
    {
        private readonly MongoService _mongo;
        public ComJobController(MongoService mongo)
        {
            _mongo = mongo;
        }

        // GET testimony detail (non-conflicting path)
        [HttpGet("info")]
        public async Task<IActionResult> GetTestimony([FromRoute] string testimonyId)
        {
            var collection = _mongo.GetCollection<JobTestimony>("job_testimony");

            var filter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId);
            var testimony = await collection.Find(filter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();

            return Ok(testimony);
        }

        // GET comments list
        [HttpGet("comments")]
        public async Task<IActionResult> GetComments([FromRoute] string job, [FromRoute] string testimonyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {

            var testimoniesColl = _mongo.GetCollection<JobTestimony>("job_testimony");
            var tFilter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId);
            var testimony = await testimoniesColl.Find(tFilter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();

            var coll = _mongo.GetCollection<JobComment>("job_testimony_comment");

            var fb = Builders<JobComment>.Filter;
            var filter = fb.Eq(c => c.JobTestimonyId, testimonyId);

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var total = await coll.CountDocumentsAsync(filter);
            var skip = (page - 1) * pageSize;
            var items = await coll.Find(filter).SortByDescending(c => c.CreatedAt).Skip(skip).Limit(pageSize).ToListAsync();

            return Ok(new { items, total });
        }

        // GET detalhes de um comentário por id
        [HttpGet("comments/{id}")]
        public async Task<IActionResult> GetCommentById([FromRoute] string job, [FromRoute] string testimonyId, string id)
        {
            var testimoniesColl = _mongo.GetCollection<JobTestimony>("job_testimony");
            var tFilter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId);
            var testimony = await testimoniesColl.Find(tFilter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();

            var coll = _mongo.GetCollection<JobComment>("job_testimony_comment");
            var filter = Builders<JobComment>.Filter.Eq(c => c.Id, id) & Builders<JobComment>.Filter.Eq(c => c.JobTestimonyId, testimonyId);
            var comment = await coll.Find(filter).FirstOrDefaultAsync();
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        // POST criar comentário
        [HttpPost("comments")]
        public async Task<IActionResult> CreateComment([FromRoute] string job, [FromRoute] string testimonyId, [FromBody] TestimonyCommentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var testimoniesColl = _mongo.GetCollection<JobTestimony>("job_testimony");
            var tFilter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId);
            var testimony = await testimoniesColl.Find(tFilter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();

            // verifica se o utilizador existe
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
                JobTestimonyId = testimonyId,
                UserId = dto.UserId ?? string.Empty,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            await coll.InsertOneAsync(comment);
            return CreatedAtAction(nameof(GetCommentById), new { job = job, testimonyId = testimonyId, id = comment.Id }, comment);
        }

        // DELETE comentário (autor)
        [HttpDelete("comments/{id}")]
        public async Task<IActionResult> DeleteComment([FromRoute] string job, [FromRoute] string testimonyId, string id)
        {
            var testimoniesColl = _mongo.GetCollection<JobTestimony>("job_testimony");
            var tFilter = Builders<JobTestimony>.Filter.Eq(t => t.Id, testimonyId);
            var testimony = await testimoniesColl.Find(tFilter).FirstOrDefaultAsync();
            if (testimony == null) return NotFound();

            var coll = _mongo.GetCollection<JobComment>("job_testimony_comment");
            var filter = Builders<JobComment>.Filter.Eq(c => c.Id, id) & Builders<JobComment>.Filter.Eq(c => c.JobTestimonyId, testimonyId);
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
