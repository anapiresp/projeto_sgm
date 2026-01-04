using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SeaRise.Interfaces
{
    public interface IComment
    {
        // Map MongoDB _id to this property
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        string Id { get; set; } 
        string UserId { get; set; }
        string Content { get; set; }
        DateTime CreatedAt { get; set; }
    }
}