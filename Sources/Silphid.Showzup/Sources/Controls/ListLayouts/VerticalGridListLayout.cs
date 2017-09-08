using Silphid.Extensions;
using UnityEngine;
// ReSharper disable PossibleLossOfFraction

namespace Silphid.Showzup.ListLayouts
{
    public class VerticalGridListLayout : GridListLayout
    {
        public int Columns;
        
        protected override Vector2 GetWrappedItemIndices(int index) =>
            new Vector2(index % Columns, index / Columns);

        protected override Vector2 GetItemSize(Vector2 viewportSize) =>
            new Vector2((viewportSize.x - (Padding.left + Padding.right) - (Columns - 1) * Spacing.x) / Columns, ItemSize.y);

        public override Vector2 GetContainerSize(int count, Vector2 viewportSize)
        {
            int rows = (count + Columns - 1) / Columns;
            return new Vector2(
                viewportSize.x,
                Padding.top + ItemSize.y * count + Spacing.y * (rows - 1).AtLeast(0) + Padding.bottom);
        }

        public override IndexRange GetVisibleIndexRange(Rect rect) =>
            new IndexRange(
                ((rect.yMin - FirstItemPosition.y) / ItemOffset.y).FloorInt() * Columns,
                ((rect.yMax - FirstItemPosition.y) / ItemOffset.y).FloorInt() * Columns + 1);
    }
}