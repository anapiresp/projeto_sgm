using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SeaRise.Models
{
    public class Offer
    {
        // Map MongoDB _id to this property
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("job_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string JobId { get; set; } = string.Empty;

        [BsonElement("company")]
        public string Company { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("media")]
        public string MediaUrl { get; set; } = string.Empty;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}