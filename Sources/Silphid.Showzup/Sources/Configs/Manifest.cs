using System.Collections.Generic;
using UnityEngine;

namespace Silphid.Showzup
{
    [CreateAssetMenu(fileName = "ShowzupManifest.asset", menuName = "Showzup/Manifest", order = 1)]
    public class Manifest : ScriptableObject
    {
        public string PrefabsPath;
        public string UriPrefix;
        public List<TypeToTypeMapping> ModelsToViewModels { get; set; }
        public List<TypeToTypeMapping> ViewModelsToViews { get; set; }
        public List<TypeToUriMapping> ViewsToPrefabs { get; set; }
    }
}