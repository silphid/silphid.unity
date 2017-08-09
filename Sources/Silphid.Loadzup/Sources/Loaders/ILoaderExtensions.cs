using System;
using Silphid.Loadzup.Caching;

namespace Silphid.Loadzup
{
    public static class ILoaderExtensions
    {
        public static IObservable<T> Load<T>(this ILoader This, string uri, Options options = null) =>
            This.Load<T>(new Uri(uri), options);

        public static IObservable<T> Load<T>(this ILoader This, Uri uri, CachePolicy cachePolicy) =>
            This.Load<T>(uri, new Options {CachePolicy = cachePolicy});

        public static IObservable<T> Load<T>(this ILoader This, string uri, CachePolicy cachePolicy) =>
            This.Load<T>(new Uri(uri), new Options {CachePolicy = cachePolicy});
    }
}