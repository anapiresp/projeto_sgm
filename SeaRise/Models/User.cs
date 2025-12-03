using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SeaRise.Services
{
    public class User
    {
        // Map MongoDB _id to this property
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("age")]
        public int Age { get; set; }

        [BsonElement("user_type")]
        public string UserType { get; set; } = string.Empty;

        [BsonElement("job")]
        public string Job { get; set; } = string.Empty;
    }
}