using System.Collections.Generic;

namespace Silphid.Showzup
{
    public class VariantGroup<T> : IVariantGroup where T : Variant<T>
    {
        private readonly List<T> _variants = new List<T>();
        private VariantSet _variantSet;

        public string Name => typeof(T).Name;
        public VariantSet Variants => _variantSet ?? (_variantSet = new VariantSet(_variants));

        public void Add(T variant)
        {
            _variants.Add(variant);
        }
    }
}