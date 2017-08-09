using System;
using System.Text;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Bundles
{
    public class BundleConverter : IConverter
    {
        public bool Supports<T>(byte[] bytes, ContentType contentType) => typeof(T) == typeof(IBundle);

        public IObservable<T> Convert<T>(byte[] bytes, ContentType contentType, Encoding encoding) =>
            AssetBundle
                .LoadFromMemoryAsync(bytes)
                .AsAsyncOperationObservable()
                .Select(x =>
                {
                    if(x.assetBundle == null)
                        throw new InvalidOperationException("Failed to convert bytes to AssetBundle");

                    return (T) (object) new AssetBundleAdaptor(x.assetBundle);
                });
    }
}