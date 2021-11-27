using Silphid.DataTypes;
using Silphid.Extensions;
using UnityEngine;

// ReSharper disable PossibleLossOfFraction

namespace Silphid.Showzup.ListLayouts
{
    public class VerticalGridListLayout : GridListLayout
    {
        public int Columns;

        protected override int RowsOrColumnsParallelToOrientation
        {
            get { return Columns; }
            set { Columns = value; }
        }

        protected override float GetVectorRelativeToOrientation(Vector2 vector, RelativeToOrientation orientation)
        {
            return orientation == RelativeToOrientation.Parallel
                       ? vector.x
                       : vector.y;
        }

        protected override void SetVectorRelativeToOrientation(ref Vector2 vector,
                                                               float value,
                                                               RelativeToOrientation orientation)
        {
            if (orientation == RelativeToOrientation.Parallel)
                vector.Set(value, vector.y);
            else
                vector.Set(vector.x, value);
        }

        protected override int GetParallelPadding(RectOffset padding)
        {
            return padding.horizontal;
        }

        protected override int GetPerpendicularPadding(RectOffset padding)
        {
            return padding.vertical;
        }

        protected override Vector2 GetContainerVector(Vector2 viewportSize, int elements)
        {
            return new Vector2(
                viewportSize.x,
                Padding.top + ItemSize.y * elements + Spacing.y * (elements - 1).AtLeast(0) + Padding.bottom);
        }

        protected override Vector2 GetWrappedItemIndices(int index) =>
            new Vector2(index % Columns, index / Columns);

        public override IntRange GetVisibleIndexRange(Rect rect) =>
            new IntRange(
                ((rect.yMin - FirstItemPosition.y + Spacing.y) / ItemOffset.y).FloorInt() * Columns,
                (((rect.yMax - FirstItemPosition.y) / ItemOffset.y).FloorInt() + 1) * Columns);
    }
}