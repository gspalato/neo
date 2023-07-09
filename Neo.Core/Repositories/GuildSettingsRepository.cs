using Neo.Common.Data;
using Neo.Common.Repositories;
using Neo.Common.Entities;

namespace Neo.Core.Repositories
{
    public interface IGuildSettingsRepository : IBaseRepository<GuildSettings>
    {
        Task<GuildSettings?> CreateGuildSettingsAsync(ulong guildId);
        Task<GuildSettings?> GetGuildSettingsAsync(ulong guildId);
    }

    public class GuildSettingsRepository : BaseRepository<GuildSettings>, IGuildSettingsRepository
    {
        public GuildSettingsRepository(IDatabaseContext dataContext) : base(dataContext)
        {

        }

        public async Task<GuildSettings?> GetGuildSettingsAsync(ulong guildId)
        {
            return await this.GetFirstByPropertyAsync("guild_id", guildId.ToString());
        }

        // Returns GuildSettings if successful, returns null if failed.
        public async Task<GuildSettings?> CreateGuildSettingsAsync(ulong guildId)
        {
            var settings = new GuildSettings
            {
                GuildId = guildId.ToString(),
                IsInRadioMode = false,
                RadioChannelId = ""
            };

            await this.InsertAsync(settings);

            return await this.GetFirstByPropertyAsync("guild_id", guildId.ToString());
        }
    }
}