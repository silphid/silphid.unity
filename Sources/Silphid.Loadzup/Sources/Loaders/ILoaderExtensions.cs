using System;
using System.Collections.Generic;
using Silphid.Loadzup.Caching;

namespace Silphid.Loadzup
{
    public static class ILoaderExtensions
    {
        public static IObservable<T> Load<T>(this ILoader This, string uri, Options options = null) =>
            This.Load<T>(new Uri(uri), options);
        
        public static ILoader With(this ILoader This, CachePolicy? cachePolicy) =>
            new CachePolicyLoaderDecorator(This, cachePolicy);
        
        public static ILoader With(this ILoader This, ContentType contentType) =>
            new ContentTypeLoaderDecorator(This, contentType);
        
        public static ILoader WithHeader(this ILoader This, string key, string value) =>
            new HeaderLoaderDecorator(This, key, value);
        
        public static ILoader WithHeaders(this ILoader This, Dictionary<string, string> headers) =>
            new HeadersLoaderDecorator(This, headers);
    }
}