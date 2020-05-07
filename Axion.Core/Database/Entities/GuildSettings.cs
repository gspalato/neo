using Canducci.MongoDB.Repository.MongoAttribute;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Axion.Core.Database.Entities
{
    public interface IGuildSettings
    {
        ObjectId Id { get; set; }

        string GuildId { get; set; }
        string Prefix { get; set; }
    }

    [MongoCollectionName("GuildSettings")]
    public class GuildSettings : IGuildSettings
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("guild_id")]
        [BsonRequired]
        public string GuildId { get; set; }

        [BsonElement("prefix")]
        [BsonRequired]
        public string Prefix { get; set; }
    }
}