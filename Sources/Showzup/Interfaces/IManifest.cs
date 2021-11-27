using System.Collections.Generic;

namespace Silphid.Showzup
{
    public interface IManifest
    {
        List<TypeToTypeMapping> ModelsToViewModels { get; }
        List<TypeToTypeMapping> ViewModelsToViews { get; }
        List<ViewToPrefabMapping> ViewsToPrefabs { get; }
    }
}