using System;

namespace Silphid.Loadzup.Bundles
{
    public interface IBundleManifestLoader
    {
        IObservable<IBundleManifest> Load();
    }
}