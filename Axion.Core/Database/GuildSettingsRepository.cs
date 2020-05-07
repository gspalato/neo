using Axion.Core.Structures.Database;
using Canducci.MongoDB.Repository.Contracts;
using Canducci.MongoDB.Repository.Connection;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Axion.Core.Database.Entities;

namespace Axion.Core.Database
{
    public interface IGuildSettingsRepository : IRepository<GuildSettings>
    {
        Task<GuildSettings> CreateForGuildAsync(ulong guildId, string prefix = null);
        Task<GuildSettings> GetForGuildAsync(ulong guildId);
        Task<GuildSettings> GetOrCreateForGuildAsync(ulong guildId, string prefix = null);
    }

    public sealed class GuildSettingsRepository : RepositoryBase<GuildSettings>, IGuildSettingsRepository
    {
        private readonly IConfiguration _configuration;

        public GuildSettingsRepository(IConfiguration configuration, IConnect connect) : base(connect)
        {
            _configuration = configuration;
        }

        public async Task<GuildSettings> CreateForGuildAsync(ulong guildId, string prefix = null)
        {
            prefix ??= _configuration.GetValue<string>("PREFIX");

            var settings = new GuildSettings
            {
                GuildId = guildId.ToString(),
                Prefix = prefix
            };

            await AddAsync(settings);

            return settings;
        }

        public async Task<GuildSettings> GetForGuildAsync(ulong guildId) =>
            await FindAsync(x => x.GuildId == guildId.ToString());

        public async Task<GuildSettings> GetOrCreateForGuildAsync(ulong guildId, string prefix = null) =>
            await GetForGuildAsync(guildId) ?? await CreateForGuildAsync(guildId, prefix);
    }
}