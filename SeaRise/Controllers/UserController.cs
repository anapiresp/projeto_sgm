using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SeaRise.Services.Database;

namespace SeaRise.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly MongoService _mongo;

        public UserController(MongoService mongo)
        {
            _mongo = mongo;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var col = _mongo.GetCollection<BsonDocument>("users");
            var doc = await col.Find(new BsonDocument()).FirstOrDefaultAsync();
            if (doc == null) return NotFound(new { message = "Sem utilizador" });
            // Don't return the password hash to clients
            if (doc.Contains("password")) doc.Remove("password");
            var json = doc.ToJson();
            return Content(json, "application/json");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("email", out var emailElem) || !body.TryGetProperty("password", out var pwdElem))
                    return BadRequest(new { ok = false, error = "email and password required" });

                var email = emailElem.GetString() ?? string.Empty;
                var password = pwdElem.GetString() ?? string.Empty;

                var col = _mongo.GetCollection<BsonDocument>("users");
                var filter = Builders<BsonDocument>.Filter.Eq("email", email);
                var user = await col.Find(filter).FirstOrDefaultAsync();
                if (user == null) return Unauthorized(new { ok = false, error = "invalid credentials" });

                if (!user.Contains("password")) return Unauthorized(new { ok = false, error = "invalid credentials" });
                var hash = user.GetValue("password").AsString;
                var ok = BCrypt.Net.BCrypt.Verify(password, hash);
                if (!ok) return Unauthorized(new { ok = false, error = "invalid credentials" });

                // Optionally remove password before returning user info
                user.Remove("password");
                return Content(user.ToJson(), "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, error = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] JsonElement body)
        {
            try
            {
                var json = body.GetRawText();
                var bson = BsonSerializer.Deserialize<BsonDocument>(json);

                var col = _mongo.GetCollection<BsonDocument>("users");

                // Build updates only for allowed fields: username, email, password
                var updates = new List<UpdateDefinition<BsonDocument>>();
                if (bson.Contains("username") && bson.GetValue("username").BsonType == BsonType.String)
                    updates.Add(Builders<BsonDocument>.Update.Set("username", bson.GetValue("username").AsString));
                if (bson.Contains("email") && bson.GetValue("email").BsonType == BsonType.String)
                    updates.Add(Builders<BsonDocument>.Update.Set("email", bson.GetValue("email").AsString));
                if (bson.Contains("password") && bson.GetValue("password").BsonType == BsonType.String)
                {
                    var rawPwd = bson.GetValue("password").AsString;
                    // Hash the password before storing
                    var hashed = BCrypt.Net.BCrypt.HashPassword(rawPwd);
                    updates.Add(Builders<BsonDocument>.Update.Set("password", hashed));
                }

                if (updates.Count == 0)
                    return BadRequest(new { ok = false, error = "Nenhum campo para atualizar" });

                UpdateDefinition<BsonDocument> combined = Builders<BsonDocument>.Update.Combine(updates);

                // Prefer filter by _id if provided
                if (bson.Contains("_id") && bson.GetValue("_id").BsonType == BsonType.String)
                {
                    var idStr = bson.GetValue("_id").AsString;
                    try
                    {
                        var objId = new ObjectId(idStr);
                        var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);
                        var result = await col.UpdateOneAsync(filter, combined);
                        if (result.MatchedCount == 0)
                        {
                            // upsert if not found
                            await col.UpdateOneAsync(filter, combined, new UpdateOptions { IsUpsert = true });
                        }
                        return Ok(new { ok = true });
                    }
                    catch (FormatException)
                    {
                        // fallthrough to other identification methods
                    }
                }

                // If no _id, try to update by email (useful even if email changed)
                if (bson.Contains("email") && bson.GetValue("email").BsonType == BsonType.String)
                {
                    var email = bson.GetValue("email").AsString;
                    var filter = Builders<BsonDocument>.Filter.Eq("email", email);
                    await col.UpdateOneAsync(filter, combined, new UpdateOptions { IsUpsert = true });
                    return Ok(new { ok = true });
                }

                // Fallback: update the first document in the collection
                var existing = await col.Find(new BsonDocument()).FirstOrDefaultAsync();
                if (existing == null)
                {
                    // create a new document with the provided fields
                    var newDoc = new BsonDocument();
                    if (bson.Contains("username")) newDoc.Set("username", bson.GetValue("username"));
                    if (bson.Contains("email")) newDoc.Set("email", bson.GetValue("email"));
                    if (bson.Contains("password")) newDoc.Set("password", bson.GetValue("password"));
                    await col.InsertOneAsync(newDoc);
                    return Ok(new { ok = true });
                }

                var idFilter2 = Builders<BsonDocument>.Filter.Eq("_id", existing.GetValue("_id"));
                await col.UpdateOneAsync(idFilter2, combined);
                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, error = ex.Message });
            }
        }
    }
}
