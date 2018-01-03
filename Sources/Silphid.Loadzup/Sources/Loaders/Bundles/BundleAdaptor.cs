using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Bundles
{
    internal class BundleAdaptor : IBundle
    {
        private readonly AssetBundle _bundle;

        public BundleAdaptor(AssetBundle bundle)
        {
            _bundle = bundle;
        }

        public IObservable<T> LoadAsset<T>(string assetName) =>
            Observable.Defer(() =>
                _bundle
                    .LoadAssetAsync<T>(assetName)
                    .AsAsyncOperationObservable()
                    .Select(x =>
                    {
                        if (x.asset == null)
                            throw new InvalidOperationException(
                                $"Failed to load asset with name \"{assetName}\" from bundle {_bundle.name} with type {typeof(T)}");

                        return (T) (object) x.asset;
                    }));

        public IObservable<T[]> LoadAllAssets<T>() =>
            Observable.Defer(() =>
                _bundle
                    .LoadAllAssetsAsync<T>()
                    .AsAsyncOperationObservable()
                    .Select(x =>
                    {
                        if (x.allAssets == null)
                            throw new InvalidOperationException(
                                $"Failed to load all asset from bundle {_bundle.name}");

                        return x.allAssets.Cast<T>().ToArray();
                    }));

        public void Unload()
        {
            _bundle.Unload(false);
        }
    }
}