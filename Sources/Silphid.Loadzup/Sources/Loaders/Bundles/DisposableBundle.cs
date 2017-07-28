using System;
using UniRx;

namespace Silphid.Loadzup.Bundles
{
    public class DisposableBundle : IBundle, IDisposable
    {
        private readonly IBundle _bundle;
        private readonly IDisposable _disposable;
        private bool _isDisposed;

        public DisposableBundle(IBundle bundle, IDisposable disposable)
        {
            _bundle = bundle;
            _disposable = disposable;
        }

        public IObservable<T> LoadAsset<T>(string assetName)
        {
            return _isDisposed 
                ? Observable.Throw<T>(new InvalidOperationException("The bundle is disposed. Cannot LoadAsset from it"))
                :_bundle.LoadAsset<T>(assetName);
        }

        public void Unload()
        {
            if (_isDisposed)
                throw new InvalidOperationException("The bundle is disposed. Cannot unload it");

            _bundle.Unload();
        }

        public void Dispose()
        {
            _isDisposed = true;
            _disposable.Dispose();
        }
    }
}