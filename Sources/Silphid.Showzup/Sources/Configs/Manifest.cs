using System.Collections.Generic;
using UnityEngine;

namespace Silphid.Showzup
{
    [CreateAssetMenu(fileName = "ShowzupManifest.asset", menuName = "Showzup/Manifest", order = 1)]
    public class Manifest : ScriptableObject, IManifest
    {
        public string PrefabsPath;
        public string UriPrefix;
        public VariantSet AllVariants;

        [SerializeField] private List<TypeToTypeMapping> _modelsToViewModels = new List<TypeToTypeMapping>();
        [SerializeField] private List<TypeToTypeMapping> _viewModelsToViews = new List<TypeToTypeMapping>();
        [SerializeField] private List<ViewToPrefabMapping> _viewsToPrefabs = new List<ViewToPrefabMapping>();

        public List<TypeToTypeMapping> ModelsToViewModels => _modelsToViewModels;
        public List<TypeToTypeMapping> ViewModelsToViews => _viewModelsToViews;
        public List<ViewToPrefabMapping> ViewsToPrefabs => _viewsToPrefabs;
    }
}