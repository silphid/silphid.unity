using UnityEngine;

namespace Silphid.Loadzup.Bundles
{
    public class AssetBundleManifestAdaptor : IManifest
    {
        private readonly AssetBundleManifest _manifest;

        public AssetBundleManifestAdaptor(AssetBundleManifest manifest)
        {
            _manifest = manifest;
        }

        public string[] GetAllDependencies(string bundleName) =>
            _manifest.GetAllDependencies(bundleName);
    }
}