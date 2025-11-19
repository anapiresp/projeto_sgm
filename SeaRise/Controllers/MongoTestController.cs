using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SeaRise.Services.Database;

namespace SeaRise.Controllers
{
    [ApiController]
    [Route("api/mongo")]
    public class MongoTestController : ControllerBase
    {
        private readonly MongoService _mongo;

        public MongoTestController(MongoService mongo)
        {
            _mongo = mongo;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var col = _mongo.GetCollection<BsonDocument>("test_collection");
            var count = await col.CountDocumentsAsync(new BsonDocument());
            return Ok(new { count });
        }

        [HttpPost("insert")]
        public async Task<IActionResult> Insert([FromBody] System.Text.Json.JsonElement? doc)
        {
            var col = _mongo.GetCollection<BsonDocument>("test_collection");

            BsonDocument toInsert;
            if (doc is null || doc.Value.ValueKind == System.Text.Json.JsonValueKind.Null)
            {
                toInsert = new BsonDocument { { "createdAt", DateTime.UtcNow }, { "note", "test" } };
            }
            else
            {
                // Convert the incoming JSON to a BsonDocument
                var json = doc.Value.GetRawText();
                toInsert = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
            }

            await col.InsertOneAsync(toInsert);

            // Return the inserted document (includes _id if generated)
            var responseJson = toInsert.ToJson();
            return new ContentResult { Content = responseJson, ContentType = "application/json", StatusCode = 201 };
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            try
            {
                var result = await _mongo.PingAsync();
                // Convert BsonDocument to a JSON string to avoid System.Text.Json trying
                // to reflect/serialize Bson types (which can cause InvalidCastException).
                var json = result.ToJson();
                var payload = "{\"ok\":true,\"result\":" + json + "}";
                return new ContentResult { Content = payload, ContentType = "application/json", StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, error = ex.Message });
            }
        }
    }
}
