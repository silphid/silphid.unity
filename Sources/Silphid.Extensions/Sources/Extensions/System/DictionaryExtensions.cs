using System;
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

        /// <summary>
        /// Tries to retrieve value from dictionary or if it does not exist, it creates one using given
        /// factory, adds it to the dictionary and returns it.
        /// </summary>
        public static TValue GetOrCreateValue<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, Func<TValue> create)
        {
            TValue value;
            if (!This.TryGetValue(key, out value))
            {
                value = create();
                This[key] = value;
            }
            
            return value;
        }

        public static IDictionary<TKey, TValue> Clone<TKey, TValue>(this IDictionary<TKey, TValue> This) =>
            This != null ? new Dictionary<TKey, TValue>(This) : null;
    }
}