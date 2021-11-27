using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Silphid.Loadzup.Caching;
using Silphid.Loadzup.Http;
using Silphid.Loadzup.Http.Caching;
using Silphid.Options;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Silphid.Loadzup
{
    public static class IOptionsExtensions
    {
        #region WithValue/Flag

        public static IOptions WithValue(this IOptions This, object key, object value) =>
            value != null
                ? new Options(This, key, value)
                : This;

        public static IOptions WithValue<T>(this IOptions This, T value) =>
            value != null
                ? new Options(This, typeof(T), value)
                : This;

        public static IOptions WithFlag(this IOptions This, object key, bool value = true) =>
            This.WithValue(key, value);

        #endregion

        #region ILoader extensions

        public static ILoader WithOptions(this ILoader This, Func<IOptions, IOptions> selector) =>
            new OptionsDecoratorLoader(This, selector);

        public static ILoader WithFlag(this ILoader This, object key, bool value = true) =>
            This.WithOptions(x => x.WithValue(key, value));

        #endregion

        #region MediaType

        private const string MediaTypeKey = "MediaType";

        public static ILoader WithMediaType(this ILoader This, string value) =>
            value != null
                ? This.WithOptions(x => x.WithMediaType(value))
                : This;

        public static IOptions WithMediaType(this IOptions This, string value) =>
            This.WithValue(MediaTypeKey, value);

        public static string GetMediaType(this IOptions This) =>
            This.GetValue<string>(MediaTypeKey, null);

        #endregion

        #region CachePolicy

        public static ILoader With(this ILoader This, CachePolicy? value) =>
            value != null
                ? This.WithOptions(x => x.WithValue(value.Value))
                : This;

        public static IOptions With(this IOptions This, CachePolicy? value) =>
            value != null
                ? This.WithValue(value.Value)
                : This;

        public static CachePolicy GetCachePolicy(this IOptions This, CachePolicy defaultValue = CachePolicy.Origin) =>
            This.GetValue(defaultValue);

        #endregion

        #region CacheGroup

        public static ILoader With(this ILoader This, CacheGroup value) =>
            value != null
                ? This.WithOptions(x => x.WithValue(value))
                : This;

        public static ILoader WithDefault(this ILoader This, CacheGroup value) =>
            value != null
                ? This.WithOptions(
                    x => x.HasValue<CacheGroup>()
                             ? x
                             : x.WithValue(value))
                : This;

        public static IOptions With(this IOptions This, CacheGroup value) =>
            value != null
                ? This.WithValue(value)
                : This;

        public static IOptions WithDefault(this IOptions This, CacheGroup value) =>
            value != null && !This.HasValue<CacheGroup>()
                ? This.WithValue(value)
                : This;

        public static CacheGroup GetCacheGroup(this IOptions This) =>
            This.GetValue<CacheGroup>();

        #endregion

        #region TimeToLive

        private const string TimeToLiveKey = "TimeToLive";

        public static ILoader WithTimeToLive(this ILoader This, TimeSpan? value) =>
            value != null
                ? This.WithOptions(x => x.WithValue(TimeToLiveKey, value.Value))
                : This;

        public static IOptions WithTimeToLive(this IOptions This, TimeSpan? value) =>
            value != null
                ? This.WithValue(TimeToLiveKey, value)
                : This;

        public static TimeSpan? GetTimeToLive(this IOptions This, TimeSpan? defaultValue = null) =>
            This.GetValue(TimeToLiveKey, defaultValue);

        #endregion

        #region MemoryCachePolicy

        public static ILoader With(this ILoader This, MemoryCachePolicy? value) =>
            value != null
                ? This.WithOptions(x => x.WithValue(value.Value))
                : This;

        public static ILoader WithMemoryCache(this ILoader This) =>
            This.With(MemoryCachePolicy.CacheOtherwiseOrigin);

        public static IOptions With(this IOptions This, MemoryCachePolicy value) =>
            This.WithValue(value);

        public static MemoryCachePolicy GetMemoryCachePolicy(this IOptions This,
                                                             MemoryCachePolicy defaultValue =
                                                                 MemoryCachePolicy.OriginOnly) =>
            This.GetValue(defaultValue);

        #endregion

        #region Headers

        private const string HeadersKey = "Headers";

        public static ILoader WithHeader(this ILoader This, string key, string value) =>
            This.WithOptions(x => x.WithHeader(key, value));

        public static ILoader WithHeaders(this ILoader This, Dictionary<string, string> values) =>
            This.WithOptions(x => x.WithHeaders(values));

        public static IOptions WithHeader(this IOptions This, string key, string value) =>
            This.WithValue(HeadersKey, new DictionaryEntry(key, value));

        public static IOptions WithHeaders(this IOptions This, Dictionary<string, string> values) =>
            This.WithValue(HeadersKey, values);

        public static IDictionary<string, string> GetHeaders(this IOptions This) =>
            This.GetValuesAsDictionary<string, string>(HeadersKey);

        #endregion

        #region HttpMethod

        public static ILoader With(this ILoader This, HttpMethod value) =>
            This.WithOptions(x => x.With(value));

        public static IOptions With(this IOptions This, HttpMethod value)
        {
            if (This.HasValue<HttpMethod>())
            {
                var existing = This.GetHttpMethod();
                if (existing != value)
                    throw new InvalidOperationException(
                        $"Cannot set HttpMethod to {value} because it has already been set to {existing}");

                return This;
            }

            return This.WithValue(value);
        }

        public static HttpMethod GetHttpMethod(this IOptions This, HttpMethod defaultValue = HttpMethod.Get) =>
            This.GetValue(defaultValue);

        #endregion

        #region Fields

        private const string PostFieldsKey = "PostFields";

        public static ILoader WithPostField(this ILoader This, string key, bool value) =>
            This.WithOptions(
                x => x.WithPostField(
                    key,
                    value
                        ? "true"
                        : "false"));

        public static ILoader WithPostField(this ILoader This, string key, string value) =>
            This.WithOptions(x => x.WithPostField(key, value));

        public static ILoader WithPostFields(this ILoader This, Dictionary<string, string> values) =>
            This.WithOptions(x => x.WithPostFields(values));

        public static IOptions WithPostField(this IOptions This, string key, string value) =>
            This.With(HttpMethod.Post)
                .WithValue(PostFieldsKey, new DictionaryEntry(key, value));

        public static IOptions WithPostFields(this IOptions This, Dictionary<string, string> values) =>
            This.With(HttpMethod.Post)
                .WithValue(PostFieldsKey, values);

        public static ILoader WithPatchFields(this ILoader This, Dictionary<string, string> values) =>
            This.WithOptions(x => x.WithPatchFields(values));

        public static IOptions WithPatchFields(this IOptions This, Dictionary<string, string> values) =>
            This.With(HttpMethod.Patch)
                .WithJsonBody(values);

        public static WWWForm GetWWWForm(this IOptions This)
        {
            var form = new WWWForm();

            foreach (DictionaryEntry value in This.GetValues(PostFieldsKey))
                form.AddField((string) value.Key, (string) value.Value);

            return form;
        }

        #endregion

        #region Body

        private const string BodyKey = "Body";

        public static ILoader WithBody(this ILoader This, string value) =>
            This.WithOptions(x => x.WithBody(value));

        public static IOptions WithBody(this IOptions This, string value)
        {
            var form = This.GetWWWForm();
            if (form != null && form.data.Length > 0)
                throw new InvalidOperationException("Cannot set body because it has already set to Form");
            if (This.GetBody() != null)
                throw new InvalidOperationException("Cannot redefine body");

            return This.With(HttpMethod.Put)
                       .WithValue(BodyKey, value);
        }

        public static string GetBody(this IOptions This) => This.GetValue<string>(BodyKey, null);

        public static ILoader WithJsonBody(this ILoader This, object value, JsonSerializerSettings settings = null) =>
            This.WithOptions(x => x.WithJsonBody(value, settings));

        public static IOptions WithJsonBody(this IOptions This, object value, JsonSerializerSettings settings = null) =>
            This.WithHeader("Content-type", "application/json")
                .WithValue(BodyKey, JsonConvert.SerializeObject(value, settings));

        public static IOptions WithPostField2(this IOptions This, string key, string value)
        {
            if (This.GetBody() != null)
                throw new InvalidOperationException("Cannot set body because it has already set");

            return This.With(HttpMethod.Post)
                       .WithHeader("Content-type", "application/x-www-form-urlencoded")
                       .WithBody($"{key}={value}");
        }

        #endregion

        #region Timeout

        private const string TimeoutKey = "Timeout";

        public static ILoader WithTimeout(this ILoader This, TimeSpan? value) =>
            This.WithOptions(x => x.WithTimeout(value));

        public static IOptions WithTimeout(this IOptions This, TimeSpan? value) =>
            This.WithValue(TimeoutKey, value);

        public static TimeSpan? GetHttpTimeout(this IOptions This) =>
            This.GetValue<TimeSpan?>(TimeoutKey, null);

        #endregion

        #region QueryParams

        private const string QueryParamsKey = "QueryParams";

        public static ILoader WithQueryParam(this ILoader This, string key, bool value) =>
            This.WithOptions(
                x => x.WithQueryParam(
                    key,
                    value
                        ? "true"
                        : "false"));

        public static ILoader WithQueryParam(this ILoader This, string key, string value) =>
            This.WithOptions(x => x.WithQueryParam(key, value));

        public static ILoader WithQueryParams(this ILoader This, Dictionary<string, string> values) =>
            This.WithOptions(x => x.WithQueryParams(values));

        public static IOptions WithQueryParam(this IOptions This, string key, string value) =>
            This.WithValue(QueryParamsKey, new DictionaryEntry(key, value));

        public static IOptions WithQueryParams(this IOptions This, Dictionary<string, string> values) =>
            This.WithValue(QueryParamsKey, values);

        public static IDictionary<string, string> GetQueryParams(this IOptions This) =>
            This.GetValuesAsDictionary<string, string>(QueryParamsKey);

        #endregion

        #region UrlEncodedBody

        public static ILoader WithUrlEncodedBody(this ILoader This, string body) =>
            This.WithOptions(x => x.WithUrlEncodedBody(body));

        public static IOptions WithUrlEncodedBody(this IOptions This, string body) =>
            This.WithHeader(KnownHeaders.ContentType, KnownMediaType.ApplicationWWWFormUrlEncoded)
                .WithBody(body);

        #endregion

        #region TextureMode

        private const string TextureModeKey = "TextureMode";

        public static IOptions WithTextureMode(this IOptions This) =>
            This.WithFlag(TextureModeKey);

        public static bool IsTextureMode(this IOptions This) =>
            This.GetFlag(TextureModeKey);

        #endregion

        #region LoadSceneMode

        public static ILoader With(this ILoader This, LoadSceneMode value) =>
            This.WithOptions(x => x.With(value));

        public static IOptions With(this IOptions This, LoadSceneMode value) =>
            This.WithValue(value);

        public static LoadSceneMode GetLoadSceneMode(this IOptions This) =>
            This.GetValue(LoadSceneMode.Additive);

        #endregion

        #region Cancelable

        public static ILoader With(this ILoader This, ICancelable value) =>
            This.WithOptions(x => x.With(value));

        public static IOptions With(this IOptions This, ICancelable value) =>
            This.WithValue(value);

        public static ICancelable GetCancelable(this IOptions This) =>
            This.GetValue<ICancelable>();

        #endregion

        #region QueuePriority

        public static ILoader With(this ILoader This, QueuePriority? priority) =>
            This.WithOptions(x => x.With(priority));

        public static IOptions With(this IOptions This, QueuePriority? value) =>
            This.WithValue(value);

        public static QueuePriority? GetQueuePriority(this IOptions This) =>
            This.GetValue<QueuePriority?>();

        #endregion
    }
}