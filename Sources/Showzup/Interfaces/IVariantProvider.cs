using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Silphid.Showzup
{
    public interface IVariantProvider
    {
        List<IVariantGroup> AllVariantGroups { get; }
        ReactiveProperty<VariantSet> GlobalVariants { get; }
    }

    public static class IVariantProviderExtensions
    {
        public static IVariant GetVariantNamed(this IVariantProvider This, string name) =>
            This.AllVariantGroups.SelectMany(x => x.Variants)
                .FirstOrDefault(x => x.Name == name);

        public static VariantSet GetVariantsNamed(this IVariantProvider This, IEnumerable<string> names) =>
            This.AllVariantGroups.SelectMany(x => x.Variants)
                .Where(x => names.Contains(x.Name))
                .ToVariantSet();
    }
}