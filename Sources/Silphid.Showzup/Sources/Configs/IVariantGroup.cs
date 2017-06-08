using System.Collections.Generic;

namespace Silphid.Showzup
{
    public interface IVariantGroup
    {
        string Name { get; }
        List<IVariant> Variants { get; }
    }
}