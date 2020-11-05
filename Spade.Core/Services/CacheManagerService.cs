using Spade.Common.Structures.Attributes;
using System;
using System.Reflection;
using System.Runtime.Caching;

namespace Spade.Core.Services
{
    public interface ICacheManagerService
    {
        T Get<T>(string key);
        void Set(string key, object data);
        bool IsSet(string key);
        string Format<T>(ulong guildId = 0, ulong userId = 0, params string[] args);
        void Remove(string key);
        void Clear();
    }

    public class CacheManagerService : ServiceBase, ICacheManagerService
    {
        private ILoggingService m_LoggingService;

        public MemoryCache Cache => _cache;
        private MemoryCache _cache;

        private CacheItemPolicy m_DefaultCacheItemPolicy = new CacheItemPolicy
        {
            SlidingExpiration = new TimeSpan(0, 30, 0)
        };

        public CacheManagerService(ILoggingService loggingService)
        {
            m_LoggingService = loggingService;

            Clear();
        }

        public T Get<T>(string key)
        {
            if (!_cache.Contains(key))
                return default;
            else
                return (T)_cache.Get(key);
        }

        public void Set(string key, object data)
        {
            if (data == null)
                return;

            _cache.Set(key, data, m_DefaultCacheItemPolicy);
        }

        public bool IsSet(string key)
            => _cache.Get(key) is not null;

        public string Format<T>(ulong guildId = 0, ulong userId = 0, params string[] args)
        {
            var entityType = typeof(T);
            var ckfaType = typeof(CacheKeyFormatAttribute);
            CacheKeyFormatAttribute ckfa = entityType.GetCustomAttribute(ckfaType) as CacheKeyFormatAttribute;

            string format = ckfa?.Format;
            if (string.IsNullOrEmpty(format))
                return "";

            var idSubstituted = format.Replace("%guild%", guildId.ToString()).Replace("%user%", userId.ToString());
            var argsFormatted = string.Format(idSubstituted, args);

            m_LoggingService.Debug($"Final cache key: {argsFormatted}");

            return argsFormatted;
        }

        public void Remove(string key)
            => _cache.Remove(key);

        public void Clear()
        {
            _cache?.Dispose();
            _cache = new MemoryCache("spade_default_cache");
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}
