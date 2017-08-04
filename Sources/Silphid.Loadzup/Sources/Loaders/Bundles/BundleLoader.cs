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
        private readonly IManifestLoader _manifestLoader;

        public bool Supports<T>(Uri uri) => uri.Scheme == Scheme.Bundle;

        public BundleLoader(IBundleCachedLoader cachedLoader, IManifestLoader manifestLoader)
        {
            _cachedLoader = cachedLoader;
            _manifestLoader = manifestLoader;
        }

        public IObservable<T> Load<T>(Uri uri, Options options = null)
        {
            // Todo only passed parsed uri
            var bundleName = uri.Host;

            return _manifestLoader.Load()
                .ContinueWith(m => LoadAllDependencies(m, bundleName, options))
                .ContinueWith(x => _cachedLoader
                    .Load(bundleName, options)
                    .DoOnError(ex => x.ForEach(y => _cachedLoader.UnloadDependency(y, bundleName)))) // Todo DRY
                    .Cast<IBundle, T>();
        }

        private IObservable<List<string>> LoadAllDependencies(IManifest manifest, string bundleName, Options options)
        {
            var loadedBundleNames = new List<string>();

            return manifest
                .GetAllDependencies(bundleName)
                .Select(x => _cachedLoader
                    .LoadDependency(x, options, bundleName)
                    .Do(y =>
                    {
                        (y as IDisposable)?.Dispose();
                        loadedBundleNames.Add(x);
                    }))
                .WhenAll()
                .Select(x => loadedBundleNames)
                .DoOnError(ex => loadedBundleNames
                    .ForEach(x => _cachedLoader.UnloadDependency(x, bundleName))); // Todo DRY
        }

        public void Unload(string bundleName)
        {
            // Bundle has not been loaded yet or does not need to be unload
            if (!_cachedLoader.Unload(bundleName))
                return;

            _manifestLoader.Load()
                .AutoDetach()
                .Subscribe(x => x
                    .GetAllDependencies(bundleName)
                    .ForEach(y => _cachedLoader.UnloadDependency(y, bundleName)));
        }
    }
}