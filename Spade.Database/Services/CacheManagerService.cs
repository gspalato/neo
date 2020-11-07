using Spade.Common.Structures;
using Spade.Common.Structures.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace Spade.Database.Services
{
    public interface ICacheManagerService
    {
        MemoryCache Cache { get; }

        T Get<T>(string key);
        void Set(string key, object data);
        bool IsSet(string key);
        string Format<T>(ulong guildId = 0, ulong userId = 0, params (string, string)[] args);
        void Remove(string key);
        void Clear();
    }

    public class CacheManagerService : ServiceBase, ICacheManagerService
    {
        public MemoryCache Cache => m_Cache;
        private MemoryCache m_Cache;

        private CacheItemPolicy m_DefaultCacheItemPolicy = new CacheItemPolicy
        {
            SlidingExpiration = new TimeSpan(0, 30, 0)
        };

        public CacheManagerService()
        {
            Clear();
        }

        public T Get<T>(string key)
        {
            if (!m_Cache.Contains(key))
                return default;
            else
                return (T)m_Cache.Get(key);
        }

        public void Set(string key, object data)
        {
            if (data == null)
                return;

            m_Cache.Set(key, data, m_DefaultCacheItemPolicy);
        }

        public bool IsSet(string key)
            => m_Cache.Get(key) is not null;

        public string Format<T>(ulong guildId = 0, ulong userId = 0, params (string, string)[] args)
        {
            var entityType = typeof(T);
            var ckfaType = typeof(CacheKeyFormatAttribute);
            CacheKeyFormatAttribute ckfa = entityType.GetCustomAttribute(ckfaType) as CacheKeyFormatAttribute;

            string format = ckfa?.Format;
            if (string.IsNullOrEmpty(format))
                return "";

            var predefinedValues = new Dictionary<string, string>()
            {
                { "guild",  guildId.ToString() },
                { "user", userId.ToString() }
            };

            var formatted = format;
            foreach (var (key, value) in predefinedValues)
                formatted = formatted.Replace(key, value);

            foreach (var (key, value) in args)
                formatted = formatted.Replace(key, value);

            return formatted;
        }

        public void Remove(string key)
            => m_Cache.Remove(key);

        public void Clear()
        {
            m_Cache?.Dispose();
            m_Cache = new MemoryCache("spade_default_cache");
        }

        public void Dispose()
        {
            m_Cache?.Dispose();
        }
    }
}
