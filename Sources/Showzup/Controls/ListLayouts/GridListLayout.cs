using System;
using Silphid.DataTypes;
using Silphid.Extensions;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Silphid.Showzup.ListLayouts
{
    public abstract class GridListLayout : ListLayout
    {
        public Vector2 Spacing;
        public Vector2 ItemSize;
        public GridLayoutAutoFillOption AutoFillOption = GridLayoutAutoFillOption.Itemsize;

        private Vector2 _adjustedSize;

        private Vector2 AdjustedSize
        {
            get
            {
                if (_adjustedSize.IsAlmostZero() && !ItemSize.IsAlmostZero())
                    return ItemSize;

                return _adjustedSize;
            }
        }

        protected Vector2 ItemOffset => AdjustedSize + Spacing;

        //depending on the layout orientation those methode will be defined to return the appropriateValue
        protected abstract int RowsOrColumnsParallelToOrientation { get; set; }
        protected abstract float GetVectorRelativeToOrientation(Vector2 vector, RelativeToOrientation orientation);

        protected abstract void SetVectorRelativeToOrientation(ref Vector2 vector,
                                                               float value,
                                                               RelativeToOrientation orientation);

        protected abstract int GetParallelPadding(RectOffset padding);
        protected abstract int GetPerpendicularPadding(RectOffset padding);
        protected abstract Vector2 GetContainerVector(Vector2 viewportSize, int count);

        #region Shortcuts

        private float GetPerpendicularVector(Vector2 vector)
        {
            return GetVectorRelativeToOrientation(vector, RelativeToOrientation.Perpendicular);
        }

        private void SetPerpendicularVector(ref Vector2 vector, float value)
        {
            SetVectorRelativeToOrientation(ref vector, value, RelativeToOrientation.Perpendicular);
        }

        private float GetParallelVector(Vector2 vector)
        {
            return GetVectorRelativeToOrientation(vector, RelativeToOrientation.Parallel);
        }

        private void SetParallelVector(ref Vector2 vector, float value)
        {
            SetVectorRelativeToOrientation(ref vector, value, RelativeToOrientation.Parallel);
        }

        /// <summary>
        /// size of the image prarallel (if oriantation horizontal -> x) to oriantation
        /// </summary>
        private float ItemSizeParallel
        {
            get { return GetParallelVector(ItemSize); }
            set { SetParallelVector(ref ItemSize, value); }
        }

        /// <summary>
        /// size of the image Perpendicular (if oriantation horizontal -> y) to oriantation
        /// </summary>
        private float ItemSizePerpendicular
        {
            get { return GetPerpendicularVector(ItemSize); }
            set { SetPerpendicularVector(ref ItemSize, value); }
        }

        private float SpacingParallel
        {
            get { return GetParallelVector(Spacing); }
            set { SetParallelVector(ref Spacing, value); }
        }

        private float SpacingPerpendicular
        {
            get { return GetPerpendicularVector(Spacing); }
            set { SetPerpendicularVector(ref Spacing, value); }
        }

        #endregion

        protected abstract Vector2 GetWrappedItemIndices(int index);

        protected Vector2 GetItemSize(Vector2 viewportSize)
        {
            if ((AutoFillOption & GridLayoutAutoFillOption.Itemsize) == GridLayoutAutoFillOption.Itemsize)
            {
                _adjustedSize = SetAndGetItemSize(viewportSize);
            }
            else
            {
                _adjustedSize = ItemSize;
            }

            return _adjustedSize;
        }

        private void AdaptColumns(Vector2 viewportSize)
        {
            var availableSpace = GetParallelVector(viewportSize) - (GetParallelPadding(Padding));

            var individualSpace = ItemSizeParallel + SpacingParallel;

            RowsOrColumnsParallelToOrientation = (int) Math.Floor(availableSpace / individualSpace);

            //if there is place for a last one. Since the spacing of the last one doesn't matter
            RowsOrColumnsParallelToOrientation += (int) Math.Floor(
                (availableSpace - RowsOrColumnsParallelToOrientation * individualSpace) / ItemSizeParallel);
        }

        protected virtual Vector2 SetAndGetItemSize(Vector2 viewportSize)
        {
            var itemVectorSize = new Vector2(
                (GetParallelVector(viewportSize) - GetParallelPadding(Padding) -
                 (RowsOrColumnsParallelToOrientation - 1) * SpacingParallel) / RowsOrColumnsParallelToOrientation,
                ItemSizePerpendicular);
            return itemVectorSize;
        }

        public override Vector2 GetContainerSize(int count, Vector2 viewportSize)
        {
            if ((AutoFillOption & GridLayoutAutoFillOption.CollumnNumber) == GridLayoutAutoFillOption.CollumnNumber)
                AdaptColumns(viewportSize);
            var rowsOrCollumnPerpendicular =
                (count + RowsOrColumnsParallelToOrientation - 1) / RowsOrColumnsParallelToOrientation;
            return GetContainerVector(viewportSize, rowsOrCollumnPerpendicular);
        }

        public override Rect GetItemRect(int index, Vector2 viewportSize) =>
            new Rect(FirstItemPosition + ItemOffset.Multiply(GetWrappedItemIndices(index)), GetItemSize(viewportSize));
    }
}