using System;
using UniRx;

namespace Silphid.Loadzup.Bundles
{
    // Use to wait for Loading of bundle (BundleCacheLoader loadingCount)
    internal class LoadingBundle : IBundle
    {
        private readonly IBundle _bundle;
        private readonly IDisposable _loadingDisposable;
        private bool _isDisposed;

        public LoadingBundle(IBundle bundle, IDisposable loadingDisposable)
        {
            _bundle = bundle;
            _loadingDisposable = loadingDisposable;
        }

        public IObservable<T> LoadAsset<T>(string assetName)
        {
            return (_isDisposed
                ? Observable.Throw<T>(new InvalidOperationException("The bundle is disposed. Cannot LoadAsset from it"))
                : _bundle.LoadAsset<T>(assetName))
                .Finally(Dispose);
        }

        public IObservable<T[]> LoadAllAssets<T>()
        {
            return (_isDisposed
                ? Observable.Throw<T[]>(
                    new InvalidOperationException("The bundle is disposed. Cannot LoadAllAssets from it."))
                : _bundle.LoadAllAssets<T>())
                .Finally(Dispose);
        }

        public void Unload()
        {
            if (_isDisposed)
                throw new InvalidOperationException("The bundle is disposed. Cannot unload it");

            _bundle.Unload();
        }

        private void Dispose()
        {
            _isDisposed = true;
            _loadingDisposable.Dispose();
        }
    }
}