using Canducci.MongoDB.Repository.MongoAttribute;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Oculus.Common.Structures.Attributes;

namespace Oculus.Database.Entities
{
	public interface IChannelWebhookEntry
	{
		ObjectId Id { get; init; }

		string ChannelId { get; init; }
		string WebhookId { get; init; }
		string WebhookToken { get; init; }
	}

	[CacheKeyFormat("WEBHOOK channel")]
	[MongoCollectionName("Webhooks")]
	public record ChannelWebhookEntry : IChannelWebhookEntry
	{
		[BsonId]
		public ObjectId Id { get; init; }

		[BsonElement("channel_id")]
		[BsonRequired]
		public string ChannelId { get; init; }

		[BsonElement("webhook_id")]
		[BsonRequired]
		public string WebhookId { get; init; }

		[BsonElement("webhook_token")]
		[BsonRequired]
		public string WebhookToken { get; init; }
	}
}