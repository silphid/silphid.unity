using System;
using Silphid.Extensions;

namespace Silphid.Showzup.ListLayouts
{
    [Flags]
    public enum GridLayoutAutoFillOption
    {
        None = 0,
        Itemsize = 1,
        CollumnNumber = 2,
        SizeAndCollumn = Itemsize | CollumnNumber
    }
}