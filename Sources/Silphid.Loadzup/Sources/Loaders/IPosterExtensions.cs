using System;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public static class IPosterExtensions
    {
        public static IObservable<T> Post<T>(this IPoster This, string uri, WWWForm form, Options options = null) =>
            This.Post<T>(new Uri(uri), form, options);

        public static IObservable<Unit> Post(this IPoster This, string uri, WWWForm form, Options options = null) =>
            This.Post<Response>(new Uri(uri), form, options)
                .AsSingleUnitObservable();
    }
}