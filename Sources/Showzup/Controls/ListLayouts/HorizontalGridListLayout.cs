using Silphid.DataTypes;
using Silphid.Extensions;
using UnityEngine;

// ReSharper disable PossibleLossOfFraction

namespace Silphid.Showzup.ListLayouts
{
    public class HorizontalGridListLayout : GridListLayout
    {
        public int Rows;

        protected override int RowsOrColumnsParallelToOrientation
        {
            get { return Rows; }
            set { Rows = value; }
        }

        protected override float GetVectorRelativeToOrientation(Vector2 vector, RelativeToOrientation orientation)
        {
            return orientation == RelativeToOrientation.Parallel
                       ? vector.y
                       : vector.x;
        }

        protected override void SetVectorRelativeToOrientation(ref Vector2 vector,
                                                               float value,
                                                               RelativeToOrientation orientation)
        {
            if (orientation == RelativeToOrientation.Parallel)
                vector.Set(vector.x, value);
            else
                vector.Set(value, vector.y);
        }

        protected override int GetParallelPadding(RectOffset padding)
        {
            return padding.vertical;
        }

        protected override int GetPerpendicularPadding(RectOffset padding)
        {
            return padding.horizontal;
        }

        protected override Vector2 GetWrappedItemIndices(int index) =>
            new Vector2(index / Rows, index % Rows);

        protected override Vector2 GetContainerVector(Vector2 viewportSize, int elements)
        {
            return new Vector2(
                Padding.left + ItemSize.x * elements + Spacing.x * (elements - 1).AtLeast(0) + Padding.right,
                viewportSize.y);
        }

        public override Vector2 GetContainerSize(int count, Vector2 viewportSize)
        {
            int columns = (count + Rows - 1) / Rows;
            return new Vector2(
                Padding.left + ItemSize.x * columns + Spacing.x * (columns - 1).AtLeast(0) + Padding.right,
                viewportSize.y);
        }

        public override IntRange GetVisibleIndexRange(Rect rect) =>
            new IntRange(
                ((rect.xMin - FirstItemPosition.x + Spacing.x) / ItemOffset.x).FloorInt() * Rows,
                (((rect.xMax - FirstItemPosition.x) / ItemOffset.x).FloorInt() + 1) * Rows);
    }
}