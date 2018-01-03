using Silphid.DataTypes;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.ListLayouts
{
    public class HorizontalListLayout : ListLayout
    {
        public float HorizontalSpacing;
        public float ItemWidth;

        protected float ItemOffsetX => ItemWidth + HorizontalSpacing;

        public override Rect GetItemRect(int index, Vector2 viewportSize) =>
            new Rect(
                FirstItemPosition + new Vector2(ItemOffsetX * index, 0),
                new Vector2(ItemWidth, viewportSize.y - (Padding.top + Padding.bottom)));

        public override Vector2 GetContainerSize(int count, Vector2 viewportSize) =>
            new Vector2(
                Padding.left + ItemWidth * count + HorizontalSpacing * (count - 1).AtLeast(0) + Padding.right,
                viewportSize.y);

        public override IntRange GetVisibleIndexRange(Rect rect) =>
            new IntRange(
                ((rect.xMin - FirstItemPosition.x + HorizontalSpacing) / ItemOffsetX).FloorInt(),
                ((rect.xMax - FirstItemPosition.x) / ItemOffsetX).FloorInt() + 1);
    }
}