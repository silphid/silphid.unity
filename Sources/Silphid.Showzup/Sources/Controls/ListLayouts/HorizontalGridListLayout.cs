using Silphid.Extensions;
using UnityEngine;
// ReSharper disable PossibleLossOfFraction

namespace Silphid.Showzup.ListLayouts
{
    public class HorizontalGridListLayout : GridListLayout
    {
        public int Rows;

        protected override Vector2 GetWrappedItemIndices(int index) =>
            new Vector2(index / Rows, index % Rows);

        protected override Vector2 GetItemSize(Vector2 viewportSize) =>
            new Vector2(
                ItemSize.x,
                (viewportSize.y - (Padding.top + Padding.bottom) - (Rows - 1) * Spacing.y) / Rows);

        public override Vector2 GetContainerSize(int count, Vector2 viewportSize)
        {
            int columns = (count + Rows - 1) / Rows;
            return new Vector2(
                Padding.left + ItemSize.x * columns + Spacing.x * (columns - 1).AtLeast(0) + Padding.right,
                viewportSize.y);
        }

        public override IndexRange GetVisibleIndexRange(Rect rect) =>
            new IndexRange( 
                ((rect.xMin - FirstItemPosition.x + Spacing.x) / ItemOffset.x).FloorInt() * Rows,
                (((rect.xMax - FirstItemPosition.x) / ItemOffset.x).FloorInt() + 1) * Rows);
    }
}