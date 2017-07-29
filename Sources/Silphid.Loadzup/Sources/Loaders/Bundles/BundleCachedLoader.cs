using System;
using System.Collections.Generic;
using System.Diagnostics;
using Silphid.Loadzup.Caching;
using UniRx;

namespace Silphid.Loadzup.Bundles
{
    public class BundleCachedLoader : CachedLoader, IBundleCachedLoader
    {
        private readonly string _baseUri;

        private static readonly Action<BundleRefCount> AddRootRefAction = refCount => refCount.AddRootRef();
        private static readonly Action<BundleRefCount> AddDependencyRefAction = refCount => refCount.AddDependencyRef();
        private static readonly Action<BundleRefCount> AddLoadingRefAction = refCount => refCount.AddLoadingRef();
        private static readonly Func<BundleRefCount, bool> ReleaseRootRefFunc = refCount => refCount.ReleaseRootRef();

        private static readonly Func<BundleRefCount, bool> ReleaseDependencyRefFunc =
            refCount => refCount.ReleaseDependencyRef();

        private static readonly Func<BundleRefCount, bool> ReleaseLoadingRefAction =
            refCount => refCount.ReleaseLoadingRef();

        #region BundleRefCount private class

        private class BundleRefCount
        {
            private IBundle _bundle;
            private int _dependencyCount;
            private int _loadingCount;
            private bool _bundleRootLoaded;

            public void SetBundle(IBundle bundle)
            {
                Debug.Assert(_bundle == null || _bundle == bundle);
                _bundle = bundle;
            }

            public bool IsRootLoaded() => _bundleRootLoaded;

            public void AddRootRef()
            {
                _bundleRootLoaded = true;
            }

            public bool ReleaseRootRef()
            {
                _bundleRootLoaded = false;
                return CheckRefCount();
            }

            public void AddDependencyRef()
            {
                _dependencyCount++;
            }

            public bool ReleaseDependencyRef()
            {
                Debug.Assert(_dependencyCount >= 1);

                _dependencyCount--;
                return CheckRefCount();
            }

            public void AddLoadingRef()
            {
                _loadingCount++;
            }

            public bool ReleaseLoadingRef()
            {
                Debug.Assert(_loadingCount >= 1);

                _loadingCount--;
                return CheckRefCount();
            }

            private bool CheckRefCount()
            {
                if (_bundleRootLoaded || _dependencyCount != 0 || _loadingCount != 0)
                    return false;

                _bundle?.Unload();

                // Do not return false if bundle is null in case where the bundle loading failed, it will still be null
                return true;
            }
        }

        #endregion

        private readonly Dictionary<string, BundleRefCount> _bundleRefCounts = new Dictionary<string, BundleRefCount>();

        private Uri GetBundleUri(string bundleName) =>
            new Uri($"{_baseUri}{bundleName}");

        public BundleCachedLoader(ILoader innerLoader, IPlatformProvider platformProvider, string baseUri)
            : base(innerLoader)
        {
            _baseUri = $"{baseUri}/{platformProvider.GetPlatformName()}/";
        }

        public IObservable<IBundle> Load(string bundleName, Options options) =>
            LoadInternal(bundleName, options, AddRootRefAction, x => Unload(x));

        public IObservable<IBundle> LoadDependency(string bundleName, Options options) =>
            LoadInternal(bundleName, options, AddDependencyRefAction, UnloadDependency);

        private IObservable<IBundle> LoadInternal(string bundleName, Options options,
            Action<BundleRefCount> addRefAction, Action<string> onErrorAction)
        {
            // Need to add ref before loading. Otherwise, if unload occurs while loading, it will release ref incorrectly
            AddRef(bundleName, addRefAction);
            AddRef(bundleName, AddLoadingRefAction);
            var releaseLoadingRefDisposable =
                Disposable.Create(() => ReleaseRef(bundleName, ReleaseLoadingRefAction, false));

            return Load<IBundle>(GetBundleUri(bundleName), options)
                .Do(x => SetBundle(x, bundleName))
                .Select(x => new DisposableBundle(x, releaseLoadingRefDisposable))
                .DoOnError(ex =>
                {
                    onErrorAction(bundleName);
                    releaseLoadingRefDisposable.Dispose();
                })
                .Cast<DisposableBundle, IBundle>();
        }

        private void SetBundle(IBundle bundle, string bundleName)
        {
            lock (this)
            {
                _bundleRefCounts[bundleName].SetBundle(bundle);
            }
        }

        public bool Unload(string bundleName) =>
            ReleaseRef(bundleName, ReleaseRootRefFunc, true);

        public void UnloadDependency(string bundleName)
        {
            ReleaseRef(bundleName, ReleaseDependencyRefFunc, false);
        }

        private void AddRef(string bundleName, Action<BundleRefCount> addRefAction)
        {
            lock (this)
            {
                BundleRefCount refCount;
                if (!_bundleRefCounts.TryGetValue(bundleName, out refCount))
                    refCount = _bundleRefCounts[bundleName] = new BundleRefCount();

                addRefAction(refCount);
            }
        }

        /// <returns>Whether the bundle was loaded or needs to be unload(</returns>
        private bool ReleaseRef(string bundleName, Func<BundleRefCount, bool> releaseRefFunc, bool isUnloadingRootRef)
        {
            lock (this)
            {
                BundleRefCount refCount;

                // No bundle to unload
                if (!_bundleRefCounts.TryGetValue(bundleName, out refCount))
                {
                    UnityEngine.Debug.Log($"Can't unload bundle named {bundleName} because it is not loaded yet");
                    return false;
                }

                // Bundle is loaded because it is only a dependency. No need to unload his dependencies again
                if (isUnloadingRootRef && !refCount.IsRootLoaded())
                    return false;

                if (releaseRefFunc(refCount))
                {
                    _bundleRefCounts.Remove(bundleName);
                    Remove(GetBundleUri(bundleName));
                }

                return true;
            }
        }
    }
}