using System;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public static class IHttpPosterExtensions
    {
        public static IObservable<T> Post<T>(this IHttpPoster This, string uri, WWWForm form, Options options = null) =>
            This.Post<T>(new Uri(uri), form, options);

        public static IObservable<Unit> Post(this IHttpPoster This, string uri, WWWForm form, Options options = null) =>
            This.Post<Response>(new Uri(uri), form, options)
                .AsSingleUnitObservable();
    }
}