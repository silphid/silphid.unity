using UnityEngine;

namespace Silphid.Loadzup.Bundles
{
    internal class BundleManifestAdaptor : IBundleManifest
    {
        private readonly AssetBundleManifest _manifest;

        public BundleManifestAdaptor(AssetBundleManifest manifest)
        {
            _manifest = manifest;
        }

        public string[] GetAllDependencies(string bundleName) =>
            _manifest.GetAllDependencies(bundleName);
    }
}