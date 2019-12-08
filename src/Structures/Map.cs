using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arpa.Structures
{
	public interface IMap<TKey, TClass>
	{
		List<TClass> Find(Func<TKey, bool> predicate);
		void Add(TKey id, TClass command);
	}

	public class Map<TKey, TClass> : IMap<TKey, TClass>
	{
		public Dictionary<TKey, TClass> dict = new Dictionary<TKey, TClass>();

		public List<TClass> Find(Func<TKey, bool> predicate)
		{
			List<TClass> found = new List<TClass>();

			foreach (TKey key in this.dict.Keys)
			{
				TClass cmd;
				if (this.dict.TryGetValue(key, out cmd))
				{
					if (predicate(key))
					{
						found.Add(cmd);
					}
				}
			}

			return found;
		}

		public void Add(TKey id, TClass command)
		{
			this.dict.Add(id, command);
		}
	}
}