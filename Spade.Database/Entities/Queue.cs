using System.Collections.Generic;
using Canducci.MongoDB.Repository.MongoAttribute;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Spade.Database.Entities
{
	public interface IQueue
	{
		ObjectId Id { get; init; }

		string GuildId { get; init; }
		IEnumerable<string> Urls { get; init; }
	}

	[MongoCollectionName("MusicQueues")]
	public record Queue : IQueue
	{
		[BsonId]
		public ObjectId Id { get; init; }

		[BsonElement("guild_id")]
		[BsonRequired]
		public string GuildId { get; init; }
		
		[BsonElement("name")]
		[BsonRequired]
		public string Name { get; init; }

		[BsonElement("urls")]
		[BsonRequired]
		public IEnumerable<string> Urls { get; init; }
	}
}