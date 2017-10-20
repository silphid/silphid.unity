using System;
using System.Text;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Bundles
{
    public class BundleConverter : ConverterBase<byte[]>
    {
        protected override bool SupportsInternal<T>(byte[] input, ContentType contentType) =>
            typeof(T) == typeof(IBundle);

        protected override IObservable<T> ConvertInternal<T>(byte[] input, ContentType contentType, Encoding encoding) =>
            AssetBundle
                .LoadFromMemoryAsync(input)
                .AsAsyncOperationObservable()
                .Select(x =>
                {
                    if(x.assetBundle == null)
                        throw new InvalidOperationException("Failed to convert bytes to AssetBundle");

                    return (T) (object) new AssetBundleAdaptor(x.assetBundle);
                });
    }
}