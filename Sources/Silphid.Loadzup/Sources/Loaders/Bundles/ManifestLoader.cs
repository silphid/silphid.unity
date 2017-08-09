using System;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Bundles
{
    public class ManifestLoader : IManifestLoader
    {
        private readonly ILoader _innerLoader;
        private const string ManifestAssetName = "AssetBundleManifest";
        private IManifest _manifest;
        private readonly Uri _manifestUri;

        public ManifestLoader(ILoader innerLoader, IPlatformProvider platformProvider, string baseUri)
        {
            _innerLoader = innerLoader;

            var platformName = platformProvider.GetPlatformName();
            _manifestUri = new Uri($"{baseUri}/{platformName}/{platformName}");
        }

        public IObservable<IManifest> Load() =>
            _manifest == null
                ? _innerLoader
                    .Load<IBundle>(_manifestUri)
                    .ContinueWith(bundle => bundle.LoadAsset<AssetBundleManifest>(ManifestAssetName))
                    .Select(x =>
                    {
                        if (x == null)
                            throw new InvalidOperationException(
                                $"No AssetBundleManifest found from manifest uri {_manifestUri}");

                        return _manifest = new AssetBundleManifestAdaptor(x);
                    })
                : Observable.Return(_manifest);
    }
}