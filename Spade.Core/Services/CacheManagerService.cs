using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Spade.Core.Services
{
	public interface ICacheManagerService
	{
		T Get<T>(string key);
		void Set(string key, object data, int cacheTime);
		bool IsSet(string key);
		void Remove(string key);
		void Clear();
	}

	public class CacheManagerService : ServiceBase, ICacheManagerService
	{
		private MemoryCache _cache;
		public CacheManagerService()
		{
			Clear();
		}

		public T Get<T>(string key)
		{
			if (!_cache.TryGetValue(key, out T value))
				return default;
			else
				return value;
		}

		public void Set(string key, object data, int cacheTime)
		{
			if (data == null)
				return;

			_cache.Set(key, data);
		}

		public bool IsSet(string key)
			=> _cache.Get(key) is not null;

		public void Remove(string key)
			=> _cache.Remove(key);

		public void Clear()
		{
			_cache?.Dispose();
			_cache = new MemoryCache(new MemoryCacheOptions());
		}

		public void Dispose()
		{
			_cache?.Dispose();
		}
	}
}
