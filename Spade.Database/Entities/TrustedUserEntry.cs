using Canducci.MongoDB.Repository.MongoAttribute;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Spade.Common.Structures.Attributes;

namespace Spade.Database.Entities
{
	public interface ITrustedUserEntry
	{
		ObjectId Id { get; init; }

		string UserId { get; init; }
	}

	[CacheKeyFormat("TRUSTEDUSER user")]
	[MongoCollectionName("TrustedUsers")]
	public record TrustedUserEntry : ITrustedUserEntry
	{
		[BsonId]
		public ObjectId Id { get; init; }

		[BsonElement("user_id")]
		[BsonRequired]
		public string UserId { get; init; }
	}
}