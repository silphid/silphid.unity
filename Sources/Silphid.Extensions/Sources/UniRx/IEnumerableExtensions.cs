using System.Collections.Generic;
using UniRx;

namespace Silphid.Extensions
{
    public static class IEnumerableUniRxExtensions
    {
        public static IEnumerable<Pair<T>> Pairwise<T>(this IEnumerable<T> This)
        {
            bool isFirst = true;
            var previous = default(T);

            foreach (var x in This)
            {
                if (!isFirst)
                    yield return new Pair<T>(x, previous);

                isFirst = false;
                previous = x;
            }
        }
    }
}