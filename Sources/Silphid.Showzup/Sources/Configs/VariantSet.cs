using System.Collections.Generic;
using JetBrains.Annotations;

namespace Silphid.Showzup
{
    public class VariantSet : HashSet<IVariant>
    {
        public VariantSet()
        {
        }

        public VariantSet(IEqualityComparer<IVariant> comparer) : base(comparer)
        {
        }

        public VariantSet([NotNull] IEnumerable<IVariant> collection) : base(collection)
        {
        }

        public VariantSet([NotNull] IEnumerable<IVariant> collection, IEqualityComparer<IVariant> comparer) : base(collection, comparer)
        {
        }

        [Pure]
        public VariantSet UnionedWith(VariantSet other)
        {
            var unioned = new VariantSet(this);
            unioned.UnionWith(other);
            return unioned;
        }
    }
}