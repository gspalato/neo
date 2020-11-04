using Canducci.MongoDB.Repository.MongoAttribute;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Spade.Database.Entities
{
	public interface ITag
	{
		ObjectId Id { get; init; }

		string GuildId { get; init; }
		string Name { get; init; }
		string Content { get; init; }
		string Author { get; init; }
		int Uses { get; init; }
	}
	
	[MongoCollectionName("Tags")]
	public record Tag : ITag
	{
		[BsonId]
		public ObjectId Id { get; init; }

		[BsonElement("guild_id")]
		[BsonRequired]
		public string GuildId { get; init; }

		[BsonElement("name")]
		[BsonRequired]
		public string Name { get; init; }

		[BsonElement("content")]
		[BsonRequired]
		public string Content { get; init; }

		[BsonElement("author")]
		[BsonRequired]
		public string Author { get; init; }

		[BsonElement("uses")]
		[BsonRequired]
		public int Uses { get; init; }
	}
}