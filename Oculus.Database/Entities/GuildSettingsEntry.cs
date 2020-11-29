using Canducci.MongoDB.Repository.MongoAttribute;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Oculus.Common.Structures.Attributes;

namespace Oculus.Database.Entities
{
	public interface IGuildSettingsEntry
	{
		ObjectId Id { get; init; }

		string GuildId { get; init; }
		string Prefix { get; init; }
	}

	[CacheKeyFormat("SETTINGS guild")]
	[MongoCollectionName("GuildSettings")]
	public record GuildSettingsEntry : IGuildSettingsEntry
	{
		[BsonId]
		public ObjectId Id { get; init; }

		[BsonElement("guild_id")]
		[BsonRequired]
		public string GuildId { get; init; }

		[BsonElement("prefix")]
		[BsonRequired]
		public string Prefix { get; init; }
	}
}