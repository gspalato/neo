using Postgrest.Attributes;
using Supabase;

namespace Oculus.Database.Models
{
    [Table("guild_settings")]
    public class GuildSettings : SupabaseModel
    {
        [PrimaryKey("guild_id")]
        public string GuildId { get; set; } = "";

        [Column("radio_mode")]
        public bool IsInRadioMode { get; set; } = false;

        [Column("radio_channel_id")]
        public string? RadioChannelId { get; set; } = "";
    }
}
