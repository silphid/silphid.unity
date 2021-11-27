using System;
using JetBrains.Annotations;
using Silphid.Extensions;
using Silphid.Showzup.Virtual.Coordinates;
using UnityEngine;

namespace Silphid.Showzup.Virtual.Layout
{
    [Serializable]
    public class LayoutInfo : ILayoutInfo
    {
        public Orientation Orientation = Orientation.Vertical;
        public Alignment AlignmentAlong = Alignment.Stretch;
        public Alignment AlignmentAcross = Alignment.Stretch;

        public Vector2 MinMargin;
        public Vector2 MaxMargin;
        public float LoadMargin = 500;

        public SizingAlong SizingAlong = SizingAlong.Variable;
        public SizingAcross SizingAcross = SizingAcross.FixedSize;
        public Vector2 Size = new Vector2(400, 300);
        public Vector2 Spacing;
        public float Ratio = 1;
        public int CountAcross = 1;
        public float TypicalSizeAlong = 300;
        public int PreloadCountAlong = 3;

        public ITransformer Transformer => Coordinates.Transformer.GetUnityToLayout(Orientation);

        #region ILayoutInfo

        Orientation ILayoutInfo.Orientation => Orientation;
        Alignment ILayoutInfo.AlignmentAlong => AlignmentAlong;
        Alignment ILayoutInfo.AlignmentAcross => AlignmentAcross;
        Vector2 ILayoutInfo.MinMargin => Transformer.TransformSize(MinMargin);
        Vector2 ILayoutInfo.MaxMargin => Transformer.TransformSize(MaxMargin);
        float ILayoutInfo.LoadMargin => LoadMargin;
        SizingAlong ILayoutInfo.SizingAlong => SizingAlong;
        SizingAcross ILayoutInfo.SizingAcross => SizingAcross;
        Vector2 ILayoutInfo.Size => Transformer.TransformSize(Size);
        Vector2 ILayoutInfo.Spacing => Transformer.TransformSize(Spacing);
        float ILayoutInfo.Ratio => Ratio;
        int ILayoutInfo.CountAcross => CountAcross;
        float ILayoutInfo.TypicalSizeAlong => TypicalSizeAlong;
        int ILayoutInfo.PreloadCountAlong => PreloadCountAlong;

        ITransformer ILayoutInfo.Transformer => Transformer;

        [Pure]
        public int GetFirstRowIndex(int index) =>
            index.AtLeast(0)
                 .FloorMultipleOf(CountAcross);

        [Pure]
        public int GetLastIndexAcross(int index) =>
            GetFirstRowIndex(index) + CountAcross - 1;
        
        #endregion
    }
}