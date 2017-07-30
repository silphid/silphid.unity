using System;

namespace Silphid.Loadzup.Bundles
{
    public interface IBundleCachedLoader
    {
        IObservable<IBundle> Load(string bundleName, Options options);
        IObservable<IBundle> LoadDependency(string bundleName, Options options, string dependencyOfBundleNamed);

        /// <returns>Whether the bundle was loaded before the call to this method</returns>
        bool Unload(string bundleName);

        void UnloadDependency(string bundleName, string dependencyOfBundleNamed);
    }
}