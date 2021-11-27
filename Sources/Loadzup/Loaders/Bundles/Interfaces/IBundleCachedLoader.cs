using System;

namespace Silphid.Loadzup.Bundles
{
    public interface IBundleCachedLoader
    {
        IObservable<IBundle> LoadBundle(string bundleName, IOptions options = null);
        IObservable<IBundle> LoadDependency(string bundleName, string dependencyOfBundleNamed, IOptions options = null);

        /// <returns>Whether the bundle was loaded before the call to this method</returns>
        bool Unload(string bundleName);

        void UnloadDependency(string bundleName, string dependencyOfBundleNamed);
    }
}