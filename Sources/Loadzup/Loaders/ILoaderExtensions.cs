using System;

namespace Silphid.Loadzup
{
    public static class ILoaderExtensions
    {
        public static IObservable<T> Load<T>(this ILoader This, string uri, IOptions options = null) =>
            This.Load<T>(new Uri(uri), options);
    }
}