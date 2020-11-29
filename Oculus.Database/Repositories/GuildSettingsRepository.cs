using Canducci.MongoDB.Repository.Connection;
using Canducci.MongoDB.Repository.Contracts;
using Microsoft.Extensions.Configuration;
using Oculus.Database.Entities;
using Oculus.Database.Services;
using System.Threading.Tasks;

namespace Oculus.Database.Repositories
{
	public interface IGuildSettingsRepository : IRepository<GuildSettingsEntry>
	{
		Task<IGuildSettingsEntry> CreateForGuildAsync(ulong guildId, string prefix = null);
		Task<IGuildSettingsEntry> GetForGuildAsync(ulong guildId);
		Task<IGuildSettingsEntry> GetOrCreateForGuildAsync(ulong guildId, string prefix = null);
		Task<IGuildSettingsEntry> UpdatePrefixAsync(ulong guildId, string prefix);
	}

	public sealed class GuildSettingsRepository : RepositoryBase<GuildSettingsEntry>, IGuildSettingsRepository
	{
		private readonly IConfiguration m_Configuration;

		private readonly ICacheManagerService m_CacheManagerService;

		public GuildSettingsRepository(IConfiguration configuration, ICacheManagerService cacheManagerService,
			IConnect connect) : base(connect)
		{
			m_Configuration = configuration;

			m_CacheManagerService = cacheManagerService;
		}

		public async Task<IGuildSettingsEntry> CreateForGuildAsync(ulong guildId, string prefix = null)
		{
			prefix ??= m_Configuration.GetValue<string>("PREFIX");

			var settings = new GuildSettingsEntry
			{
				GuildId = guildId.ToString(),
				Prefix = prefix
			};

			string cacheKey = m_CacheManagerService.Format<GuildSettingsEntry>(guildId, 0);
			m_CacheManagerService.Set(cacheKey, settings);

			return await AddAsync(settings);
		}

		public async Task<IGuildSettingsEntry> GetForGuildAsync(ulong guildId)
		{
			GuildSettingsEntry settings;

			string cacheKey = m_CacheManagerService.Format<GuildSettingsEntry>(guildId, 0);
			if (m_CacheManagerService.IsSet(cacheKey))
				settings = m_CacheManagerService.Get<GuildSettingsEntry>(cacheKey);
			else
			{
				settings = await FindAsync(x => x.GuildId == guildId.ToString());

				if (!m_CacheManagerService.IsSet(cacheKey) && settings is not null)
					m_CacheManagerService.Set(cacheKey, settings);
			}

			return settings;
		}

		public async Task<IGuildSettingsEntry> GetOrCreateForGuildAsync(ulong guildId, string prefix = null) =>
			await GetForGuildAsync(guildId) ?? await CreateForGuildAsync(guildId, prefix);

		public async Task<IGuildSettingsEntry> UpdatePrefixAsync(ulong guildId, string prefix)
		{
			if (await GetOrCreateForGuildAsync(guildId) is not GuildSettingsEntry settings)
				return default;

			var update = MongoDB.Driver.Builders<GuildSettingsEntry>.Update.Set("prefix", prefix);

			// Client prediction!
			var updatedSettings = settings with { Prefix = prefix };

			string cacheKey = m_CacheManagerService.Format<GuildSettingsEntry>(guildId, 0);
			m_CacheManagerService.Set(cacheKey, updatedSettings);

			await UpdateAsync(t => t.GuildId == guildId.ToString(), update);

			return updatedSettings;
		}
	}
}