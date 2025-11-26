using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SeaRise.Models.Interfaces
{
    public interface ITestimony
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        string UserId { get; set; }
        string Content { get; set; }
        string MediaUrl { get; set; }
        bool IsApproved { get; set; }
    }
}