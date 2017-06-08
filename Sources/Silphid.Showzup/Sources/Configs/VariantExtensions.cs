using System.Collections.Generic;

namespace Silphid.Showzup
{
    public static class VariantExtensions
    {
        public static VariantSet ToVariantSet(this IEnumerable<IVariant> This) =>
            new VariantSet(This);
    }
}