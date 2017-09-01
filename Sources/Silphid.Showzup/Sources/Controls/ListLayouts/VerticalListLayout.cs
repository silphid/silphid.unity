using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.ListLayouts
{
    public class VerticalListLayout : ListLayout
    {
        public VerticalListLayout(Vector2 itemSize, Vector2 spacing, RectOffset padding) :
            base(itemSize, spacing, padding)
        {
        }

        public override Rect GetItemRect(int index) =>
            new Rect(
                FirstItemPosition + ItemOffset.WithX(0) * index,
                ItemSize);

        public override Vector2 GetContainerSize(int count) =>
            new Vector2(
                Padding.left + ItemSize.x + Padding.right,
                Padding.top + ItemSize.y * count + Spacing.y * (count - 1).AtLeast(0) + Padding.bottom);

        public override IndexRange GetVisibleIndexRange(Rect rect) =>
            new IndexRange(
                ((rect.yMin + FirstItemPosition.y) / -ItemOffset.y).FloorInt(),
                ((rect.yMax + FirstItemPosition.y) / -ItemOffset.y).FloorInt() + 1);
    }
}