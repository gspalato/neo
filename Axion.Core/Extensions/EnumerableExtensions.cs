using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Axion.Core.Utilities
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> items, int size)
        {
            T[] array = items as T[] ?? items.ToArray();
            for (int i = 0; i < array.Length; i += size)
            {
                T[] chunk = new T[Math.Min(size, array.Length - i)];
                Array.Copy(array, i, chunk, 0, chunk.Length);
                yield return chunk;
            }
        }
    }
}
