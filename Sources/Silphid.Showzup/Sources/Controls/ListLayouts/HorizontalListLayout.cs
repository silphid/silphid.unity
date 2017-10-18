using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.ListLayouts
{
    public class HorizontalListLayout : ListLayout
    {
        public float HorizontalSpacing;
        public float ItemWidth;

        protected float ItemOffsetX => ItemWidth + HorizontalSpacing;
        private Rect VisibleRect = new Rect();

        public override Rect GetItemRect(int index, Vector2 viewportSize) =>
            new Rect(
                FirstItemPosition + new Vector2(ItemOffsetX * index, 0),
                new Vector2(ItemWidth, viewportSize.y - (Padding.top + Padding.bottom)));

        public override Vector2 GetContainerSize(int count, Vector2 viewportSize) =>
            new Vector2(
                Padding.left + ItemWidth * count + HorizontalSpacing * (count - 1).AtLeast(0) + Padding.right,
                viewportSize.y);

        public override IndexRange GetVisibleIndexRange(Rect visibleRect)
        {
            VisibleRect.Set(visibleRect.x*-1, visibleRect.y, visibleRect.width, visibleRect.height);
            return new IndexRange(
                ((VisibleRect.xMin - FirstItemPosition.x) / ItemOffsetX).FloorInt(),
                ((VisibleRect.xMax - FirstItemPosition.x) / ItemOffsetX).FloorInt() + 1);
        }
    }
}