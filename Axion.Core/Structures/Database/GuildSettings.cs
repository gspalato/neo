using Canducci.MongoDB.Repository.MongoAttribute;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Axion.Core.Structures.Database
{
	public interface IGuildSettings
	{
		ObjectId Id { get; set; }

		string guild_id { get; set; }
		string prefix { get; set; }
	}

	[MongoCollectionName("GuildSettings")]
	public class GuildSettings : IGuildSettings
	{
		[BsonId()]
		public ObjectId Id { get; set; }

		[BsonElement("guild_id")]
		[BsonRequired()]
		public string guild_id { get; set; }

		[BsonElement("prefix")]
		[BsonRequired()]
		public string prefix { get; set; }
	}
}