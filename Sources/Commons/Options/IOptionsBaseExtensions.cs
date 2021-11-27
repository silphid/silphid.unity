using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Silphid.Options
{
    public static class IOptionsBaseExtensions
    {
        #region Value

        public static IEnumerable<object> GetKeys(this IOptionsBase This) =>
            (This as IOptionsInternal)?.Keys ?? Enumerable.Empty<object>();

        public static IEnumerable<object> GetValues(this IOptionsBase This, object key) =>
            (This as IOptionsInternal)?.GetValues(key) ?? Enumerable.Empty<object>();

        public static object GetValue(this IOptionsBase This, object key) =>
            (This as IOptionsInternal)?.GetValue(key);

        public static bool HasValue(this IOptionsBase This, object key) =>
            (This as IOptionsInternal)?.HasValue(key) ?? false;

        public static bool HasValue<T>(this IOptionsBase This) =>
            This.HasValue(typeof(T));

        public static T GetValue<T>(this IOptionsBase This, object key, T defaultValue) =>
            (T) ((This as IOptionsInternal)?.GetValue(key) ?? defaultValue);

        public static T GetValue<T>(this IOptionsBase This, T defaultValue = default) =>
            (T) ((This as IOptionsInternal)?.GetValue(typeof(T)) ?? defaultValue);

        public static T GetJsonValue<T>(this IOptionsBase This, object key, T defaultValue) =>
            GetDeserializedValue<T>((This as IOptionsInternal)?.GetValue(key) ?? defaultValue);

        public static T GetJsonValue<T>(this IOptionsBase This, T defaultValue = default) =>
            GetDeserializedValue<T>((This as IOptionsInternal)?.GetValue(typeof(T)) ?? defaultValue);

        private static T GetDeserializedValue<T>(object value)
        {
            if (typeof(T) != typeof(JToken) && value is JToken jToken)
                return jToken.ToObject<T>();

            if (typeof(T).IsEnum && value is string txtValue)
                return (T) Enum.Parse(typeof(T), txtValue, true);
            return (T) value;
        }

        #endregion

        #region Values/Dictionary

        public static IEnumerable<T> GetValues<T>(this IOptionsBase This, object key) =>
            This is IOptionsInternal _this
                ? _this.GetValues(key)
                       .Cast<T>()
                : Enumerable.Empty<T>();

        public static IDictionary<TKey, TValue>
            GetValuesAsDictionary<TKey, TValue>(this IOptionsBase This, object key) =>
            This is IOptionsInternal _this
                ? _this.GetValues(key)
                       .Cast<DictionaryEntry>()
                       .ToDictionary(x => (TKey) x.Key, x => (TValue) x.Value)
                : new Dictionary<TKey, TValue>();

        #endregion

        #region Dictionary

        public static IDictionary<object, object> AsDictionary(this IOptionsBase This) =>
            This?.GetKeys()
                 .Select(x => new KeyValuePair<object, object>(x, This.GetValue(x)))
                 .ToDictionary(x => x.Key, x => x.Value);

        #endregion

        #region Flag

        public static bool GetFlag(this IOptionsBase This, object key, bool defaultValue = false) =>
            This.GetValue(key, defaultValue);

        #endregion
    }
}