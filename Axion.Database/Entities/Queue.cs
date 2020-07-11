using System.Collections.Generic;
using Canducci.MongoDB.Repository.MongoAttribute;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Axion.Database.Entities
{
	public interface IQueue
	{
		ObjectId Id { get; set; }

		string GuildId { get; set; }
		IEnumerable<string> Urls { get; set; }
	}

	[MongoCollectionName("MusicQueues")]
	public class Queue : IQueue
	{
		[BsonId]
		public ObjectId Id { get; set; }

		[BsonElement("guild_id")]
		[BsonRequired]
		public string GuildId { get; set; }
		
		[BsonElement("name")]
		[BsonRequired]
		public string Name { get; set; }

		[BsonElement("urls")]
		[BsonRequired]
		public IEnumerable<string> Urls { get; set; }
	}
}