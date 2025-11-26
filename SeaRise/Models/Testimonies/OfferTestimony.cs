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

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MediaUrl { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;

        [BsonRepresentation(BsonType.ObjectId)]
        public string OfferId { get; set; } = string.Empty;
    }
}