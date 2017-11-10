namespace Silphid.Loadzup.Bundles
{
    public interface IBundleManifest
    {
        string[] GetAllDependencies(string bundleName);
    }
}