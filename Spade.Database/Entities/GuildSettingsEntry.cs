using Canducci.MongoDB.Repository.MongoAttribute;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Spade.Database.Entities
{
	public interface IGuildSettingsEntry
	{
		ObjectId Id { get; init; }

		string GuildId { get; init; }
		string Prefix { get; init; }
	}

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