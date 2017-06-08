using System.Collections.Generic;

namespace Silphid.Showzup
{
    public class Config
    {
        public List<TypeToTypeMapping> ModelToViewModelMappings { get; } = new List<TypeToTypeMapping>();
        public List<TypeToTypeMapping> ViewModelToViewMappings { get; } = new List<TypeToTypeMapping>();
        public List<TypeToUriMapping> ViewToUriMappings { get; } = new List<TypeToUriMapping>();
        public List<Segue> Segues { get; } = new List<Segue>();
    }
}
