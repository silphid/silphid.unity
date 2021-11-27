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
        private bool _isManifestAlreadyRequested;
        private ISubject<IBundleManifest> _loadedManifest = new ReplaySubject<IBundleManifest>(1);

        public BundleManifestLoader(ILoader innerLoader, IPlatformProvider platformProvider, string baseUri)
        {
            _innerLoader = innerLoader;

            var platformName = platformProvider.GetPlatformName();
            _manifestUri = new Uri($"{baseUri}/{platformName}/{platformName}");
        }

        public IObservable<IBundleManifest> Load()
        {
            if (!_isManifestAlreadyRequested)
            {
                _isManifestAlreadyRequested = true;
                _innerLoader.Load<IBundle>(_manifestUri)
                            .ContinueWith(bundle => bundle.LoadAsset<AssetBundleManifest>(ManifestAssetName))
                            .Do(
                                 x =>
                                 {
                                     if (x == null)
                                         throw new InvalidOperationException(
                                             $"No AssetBundleManifest found from bundleManifest uri {_manifestUri}");

                                     _loadedManifest.OnNext(new BundleManifestAdaptor(x));
                                     _loadedManifest.OnCompleted();
                                 })
                            .DoOnError(
                                 ex =>
                                 {
                                     _loadedManifest.OnError(ex);
                                     _isManifestAlreadyRequested = false;
                                     _loadedManifest = new ReplaySubject<IBundleManifest>(1);
                                 })
                            .Subscribe();
            }

            return _loadedManifest;
        }
    }
}