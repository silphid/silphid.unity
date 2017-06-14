using System.Collections.Generic;
using UnityEngine;

namespace Silphid.Showzup
{
    [CreateAssetMenu(fileName = "ShowzupManifest.asset", menuName = "Showzup/Manifest", order = 1)]
    public class Manifest : ScriptableObject, IManifest
    {
        public string PrefabsPath;
        public string UriPrefix;

        [SerializeField] private List<TypeToTypeMapping> _modelsToViewModels = new List<TypeToTypeMapping>();
        [SerializeField] private List<TypeToTypeMapping> _viewModelsToViews = new List<TypeToTypeMapping>();
        [SerializeField] private List<TypeToUriMapping> _viewsToPrefabs = new List<TypeToUriMapping>();

        public List<TypeToTypeMapping> ModelsToViewModels => _modelsToViewModels;
        public List<TypeToTypeMapping> ViewModelsToViews => _viewModelsToViews;
        public List<TypeToUriMapping> ViewsToPrefabs => _viewsToPrefabs;
    }
}