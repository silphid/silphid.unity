using Silphid.Extensions;

namespace Silphid.Showzup
{
    public class VariantGroup<T> : IVariantGroup where T : Variant<T>
    {
        public string Name { get; }
        public VariantSet Variants { get; }

        public VariantGroup(params Variant<T>[] variants)
        {
            Name = typeof(T).Name;
            Variants = new VariantSet(variants);
            variants.ForEach(x => x.Group = this);
        }
    }
}