using Canducci.MongoDB.Repository.Connection;
using Canducci.MongoDB.Repository.Contracts;
using Microsoft.Extensions.Configuration;
using Muon.Kernel.Structures.Database;
using System.Threading.Tasks;

namespace Muon.Services
{
    public interface IGuildSettingsRepository : IRepository<GuildSettings>
    {
        Task<GuildSettings> GetForGuildAsync(ulong guildId);
        Task<GuildSettings> CreateForGuildAsync(ulong guildId, string prefix = null);
    }

    public sealed class GuildSettingsRepository : RepositoryBase<GuildSettings>, IGuildSettingsRepository
    {
        private readonly IConfiguration _configuration;

        public GuildSettingsRepository(IConfiguration configuration, IConnect connect) : base(connect)
        {
            _configuration = configuration;
        }

        public async Task<GuildSettings> GetForGuildAsync(ulong guildId)
        {
            return await FindAsync(x => x.guild_id == guildId.ToString());
        }

        public async Task<GuildSettings> CreateForGuildAsync(ulong guildId, string prefix = null)
        {
            prefix = prefix ?? _configuration.GetValue<string>("PREFIX");

            var settings = new GuildSettings
            {
                guild_id = guildId.ToString(),
                prefix = prefix
            };

            await AddAsync(settings);

            return settings;
        }
    }
}