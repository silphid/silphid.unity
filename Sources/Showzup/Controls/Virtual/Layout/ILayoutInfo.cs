using JetBrains.Annotations;
using Silphid.Showzup.Virtual.Coordinates;
using UnityEngine;

namespace Silphid.Showzup.Virtual.Layout
{
    public interface ILayoutInfo
    {
        Orientation Orientation { get; }
        Alignment AlignmentAlong { get; }
        Alignment AlignmentAcross { get; }

        Vector2 MinMargin { get; }
        Vector2 MaxMargin { get; }
        float LoadMargin { get; }

        SizingAlong SizingAlong { get; }
        SizingAcross SizingAcross { get; }
        Vector2 Size { get; }
        Vector2 Spacing { get; }
        float Ratio { get; }
        int CountAcross { get; }
        int PreloadCountAlong { get; }
        float TypicalSizeAlong { get; }
        
        ITransformer Transformer { get; }

        [Pure]
        int GetFirstRowIndex(int index);

        [Pure]
        int GetLastIndexAcross(int index);
    }
}