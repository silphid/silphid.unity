using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Silphid.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this Collection<T> collection, IEnumerable<T> enumerable)
        {
            foreach (T obj in enumerable)
            {
                collection.Add(obj);
            }
        }
    }
}