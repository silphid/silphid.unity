using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using Silphid.Loadzup.Caching;
using UniRx;

namespace Silphid.Loadzup.Bundles
{
    public class BundleCacheLoader : MemoryCacheLoader, IBundleCachedLoader
    {
        private readonly string _baseUri;

        private static readonly ILog Log = LogManager.GetLogger(typeof(BundleCacheLoader));

        private readonly Action<BundleRefCount, string> AddRootRefAction = (refCount, x) => refCount.AddRootRef();

        private readonly Action<BundleRefCount, string> AddDependencyRefAction =
            (refCount, dependencyWithBundleNamed) => refCount.AddDependencyRef(dependencyWithBundleNamed);

        private readonly Action<BundleRefCount, string> AddLoadingRefAction = (refCount, x) => refCount.AddLoadingRef();

        private readonly Func<BundleRefCount, string, bool> ReleaseRootRefFunc =
            (refCount, x) => refCount.ReleaseRootRef();

        private readonly Func<BundleRefCount, string, bool> ReleaseDependencyRefFunc =
            (refCount, dependencyWithBundleNamed) => refCount.ReleaseDependencyRef(dependencyWithBundleNamed);

        private readonly Func<BundleRefCount, string, bool> ReleaseLoadingRefAction =
            (refCount, x) => refCount.ReleaseLoadingRef();

        #region BundleRefCount private class

        private class BundleRefCount
        {
            private IBundle _bundle;
            private readonly List<string> _dependencyOfBundles = new List<string>();
            private int _loadingCount;

            public void SetBundle(IBundle bundle)
            {
                Debug.Assert(_bundle == null || _bundle == bundle);
                _bundle = bundle;
            }

            public bool IsRootLoaded { get; private set; }

            public void AddRootRef()
            {
                IsRootLoaded = true;
            }

            public bool ReleaseRootRef()
            {
                IsRootLoaded = false;
                return CheckRefCount();
            }

            public void AddDependencyRef(string dependencyWithBundleNamed)
            {
                lock (this)
                {
                    if (_dependencyOfBundles.IndexOf(dependencyWithBundleNamed) >= 0)
                        return;

                    _dependencyOfBundles.Add(dependencyWithBundleNamed);
                }
            }

            public bool ReleaseDependencyRef(string dependencyWithBundleNamed)
            {
                lock (this)
                {
                    _dependencyOfBundles.Remove(dependencyWithBundleNamed);
                }

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
                if (IsRootLoaded || _dependencyOfBundles.Count != 0 || _loadingCount != 0)
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

        public BundleCacheLoader(ILoader innerLoader, IPlatformProvider platformProvider, string baseUri)
            : base(innerLoader)
        {
            _baseUri = $"{baseUri}/{platformProvider.GetPlatformName()}/";
        }

        public IObservable<IBundle> LoadBundle(string bundleName, IOptions options = null) =>
            LoadInternal(bundleName, options, AddRootRefAction, (x, y) => Unload(x));

        public IObservable<IBundle> LoadDependency(string bundleName,
                                                   string dependencyWithBundleNamed,
                                                   IOptions options) =>
            LoadInternal(bundleName, options, AddDependencyRefAction, UnloadDependency, dependencyWithBundleNamed);

        private IObservable<IBundle> LoadInternal(string bundleName,
                                                  IOptions options,
                                                  Action<BundleRefCount, string> addRefAction,
                                                  Action<string, string> onErrorAction,
                                                  string dependencyWithBundleNamed = null)
        {
            // Need to add ref before loading. Otherwise, if unload occurs while loading, it will release ref incorrectly
            AddRef(bundleName, addRefAction, dependencyWithBundleNamed);
            AddRef(bundleName, AddLoadingRefAction);
            var releaseLoadingRefDisposable =
                Disposable.Create(() => ReleaseRef(bundleName, ReleaseLoadingRefAction, false));

            return Load<IBundle>(GetBundleUri(bundleName), options)
                  .Do(x => SetBundle(x, bundleName))
                  .Select(x => new LoadingBundle(x, releaseLoadingRefDisposable))
                  .DoOnError(
                       ex =>
                       {
                           onErrorAction(bundleName, dependencyWithBundleNamed);
                           releaseLoadingRefDisposable.Dispose();
                       })
                  .Cast<LoadingBundle, IBundle>();
        }

        private void SetBundle(IBundle bundle, string bundleName)
        {
            lock (this)
            {
                _bundleRefCounts[bundleName]
                   .SetBundle(bundle);
            }
        }

        public bool Unload(string bundleName) =>
            ReleaseRef(bundleName, ReleaseRootRefFunc, true);

        public void UnloadDependency(string bundleName, string dependencyWithBundleNamed)
        {
            ReleaseRef(bundleName, ReleaseDependencyRefFunc, false, dependencyWithBundleNamed);
        }

        private void AddRef(string bundleName,
                            Action<BundleRefCount, string> addRefAction,
                            string dependencyWithBundleNamed = null)
        {
            lock (this)
            {
                BundleRefCount refCount;
                if (!_bundleRefCounts.TryGetValue(bundleName, out refCount))
                    refCount = _bundleRefCounts[bundleName] = new BundleRefCount();

                addRefAction(refCount, dependencyWithBundleNamed);
            }
        }

        /// <returns>Whether needs to be unload(</returns>
        private bool ReleaseRef(string bundleName,
                                Func<BundleRefCount, string, bool> releaseRefFunc,
                                bool isUnloadingRootRef,
                                string dependencyWithBundleNamed = null)
        {
            lock (this)
            {
                BundleRefCount refCount;

                // No bundle to unload
                if (!_bundleRefCounts.TryGetValue(bundleName, out refCount))
                {
                    Log.Info($"Can't unload bundle named {bundleName} because it is not loaded yet");
                    return false;
                }

                // Bundle is loaded because it is only a dependency. No need to unload his dependencies again
                if (isUnloadingRootRef && !refCount.IsRootLoaded)
                    return false;

                if (releaseRefFunc(refCount, dependencyWithBundleNamed))
                {
                    _bundleRefCounts.Remove(bundleName);
                    Remove(GetBundleUri(bundleName));
                }

                return true;
            }
        }
    }
}