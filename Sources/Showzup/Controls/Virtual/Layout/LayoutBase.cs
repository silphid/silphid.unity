using JetBrains.Annotations;
using Silphid.DataTypes;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.Virtual.Layout
{
    public abstract class LayoutBase : ILayout
    {
        private readonly ILayoutInfo _info;
        protected readonly RangeCache _ranges;

        protected LayoutBase(ILayoutInfo info, RangeCache ranges)
        {
            _info = info;
            _ranges = ranges;
        }

        #region LayoutInfo shortcuts

        protected Orientation Orientation => _info.Orientation;
        protected Alignment AlignmentAlong => _info.AlignmentAlong;
        protected Alignment AlignmentAcross => _info.AlignmentAcross;

        protected Vector2 MinMargin => _info.MinMargin;
        protected Vector2 MaxMargin => _info.MaxMargin;
        protected float LoadMargin => _info.LoadMargin;

        protected SizingAlong SizingAlong => _info.SizingAlong;
        protected SizingAcross SizingAcross => _info.SizingAcross;
        protected Vector2 Size => _info.Size;
        protected Vector2 Spacing => _info.Spacing;
        protected float Ratio => _info.Ratio;
        protected int CountAcross => _info.CountAcross;
        protected float TypicalSizeAlong => _info.TypicalSizeAlong;
        protected int PreloadCountAlong => _info.PreloadCountAlong;

        [Pure]
        protected int GetFirstRowIndex(int index) => _info.GetFirstRowIndex(index);

        [Pure]
        protected int GetLastIndexAcross(int index) => _info.GetLastIndexAcross(index);

        #endregion

        #region Layouting

        public (float requiredHeight, float adjustment) Perform(LayoutDirection direction,
                                                                ILayoutCollection collection,
                                                                Rect viewportRect,
                                                                Vector2 availableSize)
        {
            if (_ranges.LoadedIndices.IsEmpty)
                return (availableSize.y, 0);

            var startIndex = GetStartIndex(direction, collection);
            var startPosY = GetStartPosY(startIndex, direction, viewportRect, availableSize);
            var allowedWidth = GetAllowedWidth(availableSize.x);
            var cellWidth = GetCellWidth(allowedWidth);
            var spacingX = GetSpacingX(allowedWidth);
            var (isTopLoaded, isBottomLoaded) = GetEdgesLoaded(collection, startIndex, direction);
            var directionSign = direction == LayoutDirection.Forward
                                    ? 1
                                    : -1;

//            Debug.Log(
//                $"Direction: {direction} Viewport: {viewportRect} Indices: {layoutedIndexRange} Rect: {layoutedRectRange} StartIndex: {startIndex} StartPos: {startPosY} isTopLoaded: {isTopLoaded} isBottomLoaded: {isBottomLoaded}");

            var lastPosY = startPosY;
            var currentPosY = startPosY;
            for (int i = startIndex;
                 i >= _ranges.LoadedIndices.Start && i < _ranges.LoadedIndices.End;
                 i += CountAcross * directionSign)
            {
                int endRowIndex = (i + CountAcross).AtMost(collection.Count);
                var rowHeight = GetRowHeight(i, endRowIndex, collection, availableSize);
                var posX = MinMargin.x;

                // Any item ready in that row?
                if (rowHeight.HasValue)
                {
                    var cellSize = new Vector2(cellWidth, rowHeight.Value);
                    var posY = currentPosY - (direction == LayoutDirection.Forward
                                                  ? 0
                                                  : cellSize.y);

                    for (int j = i; j < endRowIndex; j++)
                    {
                        if (collection.IsLoaded(j))
                        {
                            var layoutedRect = GetLayoutedRect(
                                collection.GetPreferredSize(j),
                                new Vector2(posX, posY),
                                cellSize);

                            collection.SetRect(j, layoutedRect);
                            _ranges.OnItemLayouted(j);
                        }

                        posX += cellWidth + spacingX;
                    }

                    currentPosY += cellSize.y * directionSign;
                    lastPosY = currentPosY;
                    currentPosY += Spacing.y * directionSign;
                }
            }

            return OnLayoutCompleted(
                direction,
                collection,
                isTopLoaded,
                isBottomLoaded,
                startPosY,
                lastPosY,
                availableSize);
        }

        protected abstract (float requiredHeight, float adjustment) OnLayoutCompleted(LayoutDirection direction,
                                                                                      ILayoutCollection collection,
                                                                                      bool isTopLoaded,
                                                                                      bool isBottomLoaded,
                                                                                      float startPosY,
                                                                                      float lastPosY,
                                                                                      Vector2 availableSize);

        protected abstract float? GetRowHeight(int start, int end, ILayoutCollection collection, Vector2 availableSize);

        protected abstract float GetStartPosY(int startIndex,
                                              LayoutDirection direction,
                                              Rect viewportRect,
                                              Vector2 availableSize);

        private int GetStartIndex(LayoutDirection direction, ILayoutCollection collection)
        {
            int index = direction == LayoutDirection.Forward
                            ? _ranges.LoadedIndices.Start
                            : _ranges.LoadedIndices.End - 1;

            // TODO: Account for viewport distance from last layouted rect to skip a certain number of items based on TypicalSizeAlong 

            return GetFirstRowIndex(index);
        }

        [Pure]
        private (bool, bool) GetEdgesLoaded(ILayoutCollection collection, int startIndex, LayoutDirection direction)
        {
            bool isTopLoaded = true, isBottomLoaded = true;

            if (direction == LayoutDirection.Forward)
            {
                for (int i = startIndex; i < collection.Count; i++)
                    if (!collection.IsLoaded(i))
                    {
                        isBottomLoaded = false;
                        break;
                    }

                isTopLoaded = startIndex == 0;
            }
            else
            {
                for (int i = startIndex; i >= 0; i--)
                    if (!collection.IsLoaded(i))
                    {
                        isTopLoaded = false;
                        break;
                    }

                isBottomLoaded = startIndex >= collection.Count - CountAcross;
            }

            return (isTopLoaded, isBottomLoaded);
        }

        [Pure]
        private Rect GetLayoutedRect(Vector2 preferredSize, Vector2 pos, Vector2 cellSize)
        {
            var preferredSizeAlong = preferredSize.y.AtMost(cellSize.y);
            var preferredSizeAcross = preferredSize.x.AtMost(cellSize.x);

            var sizeAlong = AlignmentAlong == Alignment.Stretch
                                ? cellSize.y
                                : preferredSizeAlong;
            var sizeAcross = AlignmentAcross == Alignment.Stretch
                                 ? cellSize.x
                                 : preferredSizeAcross;

            var minAlong = pos.y + (AlignmentAlong == Alignment.Stretch || AlignmentAlong == Alignment.Min
                                        ? 0
                                        : AlignmentAlong == Alignment.Max
                                            ? cellSize.y - sizeAlong
                                            : (cellSize.y - sizeAlong) / 2);
            var minAcross = pos.x + (AlignmentAcross == Alignment.Stretch || AlignmentAcross == Alignment.Min
                                         ? 0
                                         : AlignmentAcross == Alignment.Max
                                             ? cellSize.x - sizeAcross
                                             : (cellSize.x - sizeAcross) / 2);

            return new Rect(minAcross, minAlong, sizeAcross, sizeAlong);
        }

        protected float GetAllowedWidth(float availableSizeAcross) =>
            availableSizeAcross - MinMargin.x - MaxMargin.x;

        protected float GetCellWidth(float allowedSizeAcross) =>
            SizingAcross == SizingAcross.FixedSize
                ? Size.x
                : (allowedSizeAcross - (CountAcross - 1) * Spacing.x) / CountAcross;

        protected float GetSpacingX(float allowedSizeAcross) =>
            SizingAcross == SizingAcross.FixedSize
                ? (allowedSizeAcross - CountAcross * Size.x) / (CountAcross - 1)
                : Spacing.x;

        #endregion

        #region GetActiveRange

        public abstract IntRange GetActiveRange(ILayoutCollection collection, Rect viewportRect, Vector2 availableSize);

        #endregion

        [Pure]
        protected IntRange GetRangeRoundedToWholeRows(IntRange range) =>
            new IntRange(GetFirstRowIndex(range.Start), GetLastIndexAcross(range.End - 1) + 1);
    }
}