using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Showzup
{
    public class VariantGroup<T> : IVariantGroup where T : Variant<T>
    {
        private readonly List<IVariant> _untypedVariants;

        public string Name { get; }
        List<IVariant> IVariantGroup.Variants => _untypedVariants;
        public List<Variant<T>> Variants { get; }

        public VariantGroup(params Variant<T>[] variants)
        {
            Name = typeof(T).Name;
            Variants = variants.ToList();
            _untypedVariants = variants.Cast<IVariant>().ToList();
            variants.ForEach(x => x.Group = this);
        }
    }
}