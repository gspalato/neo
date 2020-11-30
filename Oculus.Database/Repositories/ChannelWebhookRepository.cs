using Canducci.MongoDB.Repository.Connection;
using Oculus.Database.Entities;
using Oculus.Database.Services;
using System.Threading.Tasks;

namespace Oculus.Database.Repositories
{
	public interface IChannelWebhookRepository
	{
		Task<IChannelWebhookEntry> GetForChannelAsync(ulong channelId);
		Task<IChannelWebhookEntry> AddWebhookAsync(ulong channelId, ulong webhookId, string webhookToken);
		Task RemoveWebhookAsync(ulong channelId);
	}

	public class ChannelWebhookRepository : RepositoryBase<ChannelWebhookEntry>, IChannelWebhookRepository
	{
		private readonly ICacheManagerService m_CacheManagerService;

		public ChannelWebhookRepository(ICacheManagerService cacheManagerService, IConnect connect)
			: base(connect)
		{
			m_CacheManagerService = cacheManagerService;
		}

		public async Task<IChannelWebhookEntry> GetForChannelAsync(ulong channelId)
		{
			ChannelWebhookEntry webhook;

			string cacheKey = m_CacheManagerService.Format<ChannelWebhookEntry>(0, 0,
				("channel", channelId.ToString()));
			if (m_CacheManagerService.IsSet(cacheKey))
				webhook = m_CacheManagerService.Get<ChannelWebhookEntry>(cacheKey);
			else
			{
				webhook = await FindAsync(x => x.ChannelId == channelId.ToString());

				if (!m_CacheManagerService.IsSet(cacheKey) && webhook is not null)
					m_CacheManagerService.Set(cacheKey, webhook);
			}

			return webhook;
		}

		public async Task<IChannelWebhookEntry> AddWebhookAsync(ulong channelId, ulong webhookId,
			string webhookToken)
		{
			var webhook = new ChannelWebhookEntry
			{
				ChannelId = channelId.ToString(),
				WebhookId = webhookId.ToString(),
				WebhookToken = webhookToken
			};

			string cacheKey = m_CacheManagerService.Format<ChannelWebhookEntry>(0, 0, 
				("channel", channelId.ToString()));
			m_CacheManagerService.Set(cacheKey, webhook);

			return await AddAsync(webhook);
		}

		public async Task RemoveWebhookAsync(ulong channelId)
		{
			string cacheKey = m_CacheManagerService.Format<ChannelWebhookEntry>(0, 0,
				("channel", channelId.ToString()));
			m_CacheManagerService.Remove(cacheKey);

			await DeleteAsync(u => u.ChannelId == channelId.ToString());
		}
	}
}
