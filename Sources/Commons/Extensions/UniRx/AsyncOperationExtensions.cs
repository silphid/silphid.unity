using System;
using System.Collections;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silphid.Extensions
{
    public static class AsyncOperationExtensions
    {
        public static IObservable<TAsset> AsObservable<TAsset>(this ResourceRequest This) where TAsset : Object =>
            Observable.FromCoroutine<TAsset>((observer, _) => AsObservableCore(This, observer));

        private static IEnumerator AsObservableCore<TAsset>(ResourceRequest This, IObserver<TAsset> observer)
            where TAsset : Object
        {
            while (!This.isDone)
                yield return null;

            var asset = This.asset as TAsset;
            if (asset == null)
            {
                observer.OnError(
                    new Exception(
                        This.asset == null
                            ? $"Asset of type {typeof(TAsset).Name} not found."
                            : $"Expected asset type was {typeof(TAsset).Name} but actual loaded type is {This.asset.GetType().Name}."));
            }
            else
            {
                observer.OnNext(asset);
                observer.OnCompleted();
            }
        }
    }
}