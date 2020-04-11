using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Muon.Kernel.Utilities
{
    public static class EnumerableExtensions
    {
        public static ValueTask<List<TSource>> ToListAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source is IAsyncIListProvider<TSource> listProvider)
                return listProvider.ToListAsync(cancellationToken);

            return Core(source, cancellationToken);

            static async ValueTask<List<TSource>> Core(IAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
            {
                var list = new List<TSource>();

                await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    list.Add(item);
                }

                return list;
            }
        }
    }
}
