using System;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Bundles
{
    public class AssetBundleAdaptor : IBundle
    {
        private readonly AssetBundle _bundle;

        public AssetBundleAdaptor(AssetBundle bundle)
        {
            _bundle = bundle;
        }

        public IObservable<T> LoadAsset<T>(string assetName) =>
            _bundle
                .LoadAssetAsync<T>(assetName)
                .AsAsyncOperationObservable()
                .Select(request =>
                {
                    if (request.asset == null)
                        throw new InvalidOperationException(
                            $"Failed to load asset with name \"{assetName}\" from bundle {_bundle.name} with type {typeof(T)}");

                    return (T) (object) request.asset;
                });

        public void Unload()
        {
            _bundle.Unload(false);
        }
    }
}