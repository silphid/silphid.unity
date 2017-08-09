using System.Collections.Generic;

namespace Silphid.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            if (This.TryGetValue(key, out value))
                return value;

            return defaultValue;
        }
    }
}