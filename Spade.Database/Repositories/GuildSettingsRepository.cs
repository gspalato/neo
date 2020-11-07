using Spade.Database.Entities;
using Canducci.MongoDB.Repository.Connection;
using Canducci.MongoDB.Repository.Contracts;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Spade.Database.Services;

namespace Spade.Database.Repositories
{
	public interface IGuildSettingsRepository : IRepository<GuildSettingsEntry>
	{
		Task<IGuildSettingsEntry> CreateForGuildAsync(ulong guildId, string prefix = null);
		Task<IGuildSettingsEntry> GetForGuildAsync(ulong guildId);
		Task<IGuildSettingsEntry> GetOrCreateForGuildAsync(ulong guildId, string prefix = null);
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
	}
}