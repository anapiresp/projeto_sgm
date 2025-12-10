using SeaRise.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SeaRise.Models
{
    public class OfferComment : IComment
    {
        // Map MongoDB _id to this property
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("content")]
        public string Content { get; set; } = string.Empty;

        [BsonElement("created_at")] 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("offer_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string OfferId { get; set; } = string.Empty;
    }
}