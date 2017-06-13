using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Silphid.Showzup
{
    public class VariantProvider : IVariantProvider
    {
        public List<IVariantGroup> AllVariantGroups { get; }
        public ReactiveProperty<VariantSet> GlobalVariants { get; } =
            new ReactiveProperty<VariantSet>(VariantSet.Empty);

        public VariantProvider(params IVariantGroup[] allVariantGroups)
        {
            AllVariantGroups = allVariantGroups.ToList();
        }
    }
}