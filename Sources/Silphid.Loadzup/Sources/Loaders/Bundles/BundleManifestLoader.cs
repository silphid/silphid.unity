using System;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Bundles
{
    public class BundleManifestLoader : IBundleManifestLoader
    {
        private readonly ILoader _innerLoader;
        private const string ManifestAssetName = "AssetBundleManifest";
        private IBundleManifest _bundleManifest;
        private readonly Uri _manifestUri;

        public BundleManifestLoader(ILoader innerLoader, IPlatformProvider platformProvider, string baseUri)
        {
            _innerLoader = innerLoader;

            var platformName = platformProvider.GetPlatformName();
            _manifestUri = new Uri($"{baseUri}/{platformName}/{platformName}");
        }

        public IObservable<IBundleManifest> Load() =>
            _bundleManifest == null
                ? _innerLoader
                    .Load<IBundle>(_manifestUri)
                    .ContinueWith(bundle => bundle.LoadAsset<AssetBundleManifest>(ManifestAssetName))
                    .Select(x =>
                    {
                        if (x == null)
                            throw new InvalidOperationException(
                                $"No AssetBundleManifest found from bundleManifest uri {_manifestUri}");

                        return _bundleManifest = new BundleManifestAdaptor(x);
                    })
                : Observable.Return(_bundleManifest);
    }
}