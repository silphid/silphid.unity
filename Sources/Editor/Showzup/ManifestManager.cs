using System.Linq;
using UnityEditor;

namespace Silphid.Showzup.Editor
{
    public class ManifestManager
    {
        private static Manifest _manifest;
        public static Manifest Manifest => _manifest ?? (_manifest = LoadManifest());

        private static Manifest LoadManifest()
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(Manifest)}");
            if (!guids.Any())
                ScriptableObjectUtility.Create<Manifest>("Showzup Manifest");

            var assetPath = AssetDatabase.GUIDToAssetPath(guids.FirstOrDefault());
            return AssetDatabase.LoadAssetAtPath<Manifest>(assetPath);
        }
    }
}