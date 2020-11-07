using Spade.Database.Entities;
using Canducci.MongoDB.Repository.Connection;
using Canducci.MongoDB.Repository.Contracts;
using Discord;
using System.Threading.Tasks;
using Spade.Database.Services;
using System;

namespace Spade.Database.Repositories
{
    public interface ITrustedUserRepository
    {
        Task<bool> IsTrusted(ulong userId);

        Task<ITrustedUserEntry> AddTrustedUserAsync(ulong userId);
        Task RemoveTrustedUserAsync(ulong userId);
    }

    public class TrustedUserRepository : RepositoryBase<TrustedUserEntry>, ITrustedUserRepository
    {
        private ICacheManagerService m_CacheManagerService;

        public TrustedUserRepository(ICacheManagerService cacheManagerService, IConnect connect) : base(connect)
        {
            m_CacheManagerService = cacheManagerService;
        }

        public async Task<bool> IsTrusted(ulong userId)
        {
            bool isTrusted;

            string cacheKey = m_CacheManagerService.Format<TrustedUserEntry>(0, userId);

            if (m_CacheManagerService.IsSet(cacheKey))
            {
                isTrusted = m_CacheManagerService.Get<TrustedUserEntry>(cacheKey) is not null;
            }
            else
            {
                var databaseValue = await FindAsync(tu => tu.UserId == userId.ToString());

                isTrusted = databaseValue is not null;

                if (!m_CacheManagerService.IsSet(cacheKey) && isTrusted)
                    m_CacheManagerService.Set(cacheKey, databaseValue);
            }

            return isTrusted;
        }

        public async Task<ITrustedUserEntry> AddTrustedUserAsync(ulong userId)
        {
            var trusted = new TrustedUserEntry
            {
                UserId = userId.ToString()
            };

            string cacheKey = m_CacheManagerService.Format<TrustedUserEntry>(0, userId);
            m_CacheManagerService.Set(cacheKey, trusted);

            return await AddAsync(trusted);
        }

        public async Task RemoveTrustedUserAsync(ulong userId)
        {
            string cacheKey = m_CacheManagerService.Format<TrustedUserEntry>(0, userId);
            m_CacheManagerService.Remove(cacheKey);

            await DeleteAsync(u => u.UserId == userId.ToString());
        }
    }
}
