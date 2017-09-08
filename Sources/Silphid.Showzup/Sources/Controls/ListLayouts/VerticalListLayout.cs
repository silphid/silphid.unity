using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.ListLayouts
{
    public class VerticalListLayout : ListLayout
    {
        public float VerticalSpacing;
        public float ItemHeight;

        protected float ItemOffsetY => ItemHeight + VerticalSpacing;

        public override Rect GetItemRect(int index, Vector2 viewportSize) =>
            new Rect(
                FirstItemPosition + new Vector2(0, ItemOffsetY * index),
                new Vector2(viewportSize.x - (Padding.left + Padding.right), ItemHeight));

        public override Vector2 GetContainerSize(int count, Vector2 viewportSize) =>
            new Vector2(
                viewportSize.x,
                Padding.top + ItemHeight * count + VerticalSpacing * (count - 1).AtLeast(0) + Padding.bottom);

        public override IndexRange GetVisibleIndexRange(Rect rect) =>
            new IndexRange(
                ((rect.yMin + FirstItemPosition.y) / -ItemOffsetY).FloorInt(),
                ((rect.yMax + FirstItemPosition.y) / -ItemOffsetY).FloorInt() + 1);
    }
}