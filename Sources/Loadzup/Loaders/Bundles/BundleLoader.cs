using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Loadzup.Bundles
{
    public class BundleLoader : ILoader, IBundleUnloader
    {
        private readonly IBundleCachedLoader _cachedLoader;
        private readonly IBundleManifestLoader _bundleManifestLoader;

        public bool Supports<T>(Uri uri) => uri.Scheme == Scheme.Bundle;

        public BundleLoader(IBundleCachedLoader cachedLoader, IBundleManifestLoader bundleManifestLoader)
        {
            _cachedLoader = cachedLoader;
            _bundleManifestLoader = bundleManifestLoader;
        }

        public IObservable<T> Load<T>(Uri uri, IOptions options = null)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            // Todo only passed parsed uri
            var bundleName = uri.Host;

            return _bundleManifestLoader.Load()
                                        .ContinueWith(m => LoadAllDependencies(m, bundleName, options))
                                        .ContinueWith(
                                             x => _cachedLoader.LoadBundle(bundleName, options)
                                                               .DoOnError(
                                                                    ex => x.ForEach(
                                                                        y => _cachedLoader.UnloadDependency(
                                                                            y,
                                                                            bundleName)))) // Todo DRY
                                        .Cast<IBundle, T>();
        }

        private IObservable<List<string>> LoadAllDependencies(IBundleManifest bundleManifest,
                                                              string bundleName,
                                                              IOptions options)
        {
            var loadedBundleNames = new List<string>();

            return bundleManifest.GetAllDependencies(bundleName)
                                 .Select(
                                      x => _cachedLoader.LoadDependency(x, bundleName, options)
                                                        .Do(
                                                             y =>
                                                             {
                                                                 (y as IDisposable)?.Dispose();
                                                                 loadedBundleNames.Add(x);
                                                             }))
                                 .WhenAll()
                                 .Select(x => loadedBundleNames)
                                 .DoOnError(
                                      ex => loadedBundleNames.ForEach(
                                          x => _cachedLoader.UnloadDependency(x, bundleName))); // Todo DRY
        }

        public void Unload(string bundleName)
        {
            // Bundle has not been loaded yet or does not need to be unload
            if (!_cachedLoader.Unload(bundleName))
                return;

            _bundleManifestLoader.Load()
                                 .AutoDetach()
                                 .Subscribe(
                                      x => x.GetAllDependencies(bundleName)
                                            .ForEach(y => _cachedLoader.UnloadDependency(y, bundleName)));
        }
    }
}