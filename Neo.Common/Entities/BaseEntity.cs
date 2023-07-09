using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Neo.Common.Entities
{
    public class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;
    }
}