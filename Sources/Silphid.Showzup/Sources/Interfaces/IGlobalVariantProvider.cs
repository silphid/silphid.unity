using System.Collections.Generic;

namespace Silphid.Showzup
{
    public interface IGlobalVariantProvider
    {
        IEnumerable<string> Variants { get; }
    }
}