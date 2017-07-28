using System;

namespace Silphid.Loadzup.Bundles
{
    public interface IManifestLoader
    {
        IObservable<IManifest> Load();
    }
}