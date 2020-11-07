using Spade.Database.Entities;
using Canducci.MongoDB.Repository.Connection;
using Canducci.MongoDB.Repository.Contracts;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

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

		public GuildSettingsRepository(IConfiguration configuration, IConnect connect) : base(connect)
		{
			m_Configuration = configuration;
		}

		public async Task<IGuildSettingsEntry> CreateForGuildAsync(ulong guildId, string prefix = null)
		{
			prefix ??= m_Configuration.GetValue<string>("PREFIX");

			var settings = new GuildSettingsEntry
			{
				GuildId = guildId.ToString(),
				Prefix = prefix
			};

			return await AddAsync(settings);
		}

		public async Task<IGuildSettingsEntry> GetForGuildAsync(ulong guildId) =>
			await FindAsync(x => x.GuildId == guildId.ToString());

		public async Task<IGuildSettingsEntry> GetOrCreateForGuildAsync(ulong guildId, string prefix = null) =>
			await GetForGuildAsync(guildId) ?? await CreateForGuildAsync(guildId, prefix);
	}
}