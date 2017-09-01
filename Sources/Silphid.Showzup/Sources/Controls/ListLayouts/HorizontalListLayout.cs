using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.ListLayouts
{
    public class HorizontalListLayout : ListLayout
    {
        public HorizontalListLayout(Vector2 itemSize, Vector2 spacing, RectOffset padding) :
            base(itemSize, spacing, padding)
        {
        }

        public override Rect GetItemRect(int index) =>
            new Rect(
                FirstItemPosition + ItemOffset.WithY(0) * index,
                ItemSize);

        public override Vector2 GetContainerSize(int count) =>
            new Vector2(
                Padding.left + ItemSize.x * count + Spacing.x * (count - 1).AtLeast(0) + Padding.right,
                Padding.top + ItemSize.y + Padding.bottom);

        public override IndexRange GetVisibleIndexRange(Rect rect) =>
            new IndexRange(
                ((rect.xMin - FirstItemPosition.x) / ItemOffset.x).FloorInt(),
                ((rect.xMax - FirstItemPosition.x) / ItemOffset.x).FloorInt() + 1);
    }
}