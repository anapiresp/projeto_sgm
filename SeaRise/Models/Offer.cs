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
    }
}