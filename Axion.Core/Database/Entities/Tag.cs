using Canducci.MongoDB.Repository.MongoAttribute;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Axion.Core.Database.Entities
{
	public interface ITag
	{
		ObjectId Id { get; set; }

		string GuildId { get; set; }
		string Name { get; set; }
		string Content { get; set; }
		string Author { get; set; }
		int Uses { get; set; }
	}

	[MongoCollectionName("Tags")]
	public class Tag : ITag
	{
		[BsonId]
		public ObjectId Id { get; set; }

		[BsonElement("guild_id")]
		[BsonRequired]
		public string GuildId { get; set; }

		[BsonElement("name")]
		[BsonRequired]
		public string Name { get; set; }

		[BsonElement("content")]
		[BsonRequired]
		public string Content { get; set; }

		[BsonElement("Author")]
		[BsonRequired]
		public string Author { get; set; }

		[BsonElement("Uses")]
		[BsonRequired]
		public int Uses { get; set; }
	}
}