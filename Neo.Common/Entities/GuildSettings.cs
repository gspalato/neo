using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Neo.Common.Entities
{
    public class GuildSettings : BaseEntity
    {
        [BsonElement("guild_id")]
        public string GuildId { get; set; } = "";

        [BsonElement("radio_mode")]
        public bool IsInRadioMode { get; set; } = false;

        [BsonElement("radio_channel_id")]
        public string? RadioChannelId { get; set; } = "";
    }
}
