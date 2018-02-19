using System.Collections.Generic;

namespace Silphid.Showzup
{
    public class Config
    {
        public List<TypeToTypeMapping> ModelToViewModelMappings { get; } = new List<TypeToTypeMapping>();
        public List<TypeToTypeMapping> ViewModelToViewMappings { get; } = new List<TypeToTypeMapping>();
        public List<ViewToPrefabMapping> ViewToUriMappings { get; } = new List<ViewToPrefabMapping>();
        public List<Segue> Segues { get; } = new List<Segue>();
    }
}
