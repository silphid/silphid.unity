namespace Silphid.Loadzup.Bundles
{
    public interface IManifest
    {
        string[] GetAllDependencies(string bundleName);
    }
}