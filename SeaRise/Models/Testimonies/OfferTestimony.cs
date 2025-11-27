using SeaRise.Models.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace SeaRise.Models
{
    public class OfferTestimony : ITestimony
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

        [BsonElement("media")]
        public string MediaUrl { get; set; } = string.Empty;

        [BsonElement("is_approved")]
        public bool IsApproved { get; set; } = false;

        [BsonElement("was_rejected")]
        public bool WasRejected { get; set; } = false;

        [BsonElement("offer_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string OfferId { get; set; } = string.Empty;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}