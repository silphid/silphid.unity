using System.Collections.Generic;
using UnityEngine;

namespace Silphid.Showzup
{
    [CreateAssetMenu(fileName = "ShowzupManifest.asset", menuName = "Showzup/Manifest", order = 1)]
    public class Manifest : ScriptableObject
    {
        public string PrefabsPath;
        public string UriPrefix;
        public List<TypeToUriMapping> TypeToUriMappings { get; set; }
    }
}