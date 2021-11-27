using JetBrains.Annotations;
using Silphid.DataTypes;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.Virtual.Layout
{
    public class VariableLayout : LayoutBase
    {
        public VariableLayout(ILayoutInfo info, RangeCache ranges)
            : base(info, ranges) {}

        protected override float? GetRowHeight(int start, int end, ILayoutCollection collection, Vector2 availableSize)
        {
            float? max = null;

            for (int i = start; i < end; i++)
            {
                if (collection.IsLoaded(i))
                {
                    var size = collection.GetPreferredSize(i)
                                         .y;

                    if (!max.HasValue || size > max)
                        max = size;
                }
            }

            return max;
        }

        protected override float GetStartPosY(int startIndex,
                                              LayoutDirection direction,
                                              Rect viewportRect,
                                              Vector2 availableSize) =>
            direction == LayoutDirection.Forward
                ? !_ranges.LayoutedIndices.IsEmpty
                      ? _ranges.LayoutedRect.min.y
                      : MinMargin.y.Max(viewportRect.min.y)
                : !_ranges.LayoutedIndices.IsEmpty
                    ? _ranges.LayoutedRect.max.y
                    : (availableSize.y - MaxMargin.y).Min(viewportRect.max.y);

        protected override (float requiredHeight, float adjustment) OnLayoutCompleted(LayoutDirection direction,
                                                                                      ILayoutCollection collection,
                                                                                      bool isTopLoaded,
                                                                                      bool isBottomLoaded,
                                                                                      float startPosY,
                                                                                      float lastPosY,
                                                                                      Vector2 availableSize)
        {
            var topLoadMargin = isTopLoaded
                                    ? 0
                                    : LoadMargin;
            var bottomLoadMargin = isBottomLoaded
                                       ? 0
                                       : LoadMargin;
            var topMargin = MinMargin.y + topLoadMargin;
            var topRowY = direction == LayoutDirection.Forward
                              ? startPosY
                              : lastPosY;

            var adjustment = AdjustToStart(collection, topRowY, topMargin);
            lastPosY += adjustment;
            startPosY += adjustment;

            var requiredHeight = direction == LayoutDirection.Forward
                                     ? lastPosY + MaxMargin.y + bottomLoadMargin
                                     : startPosY + MaxMargin.y + bottomLoadMargin;

            return (requiredHeight, adjustment);
        }

        private float AdjustToStart(ILayoutCollection collection, float lastPosY, float topMargin)
        {
            var adjustment = new Vector2(0, topMargin - lastPosY);

            foreach (var i in _ranges.LayoutedIndices)
            {
                if (collection.IsLayouted(i))
                {
                    var rect = collection.GetRect(i);
                    collection.SetRect(i, rect.Translate(adjustment));
                }
            }

            return adjustment.y;
        }

        #region GetActiveRange

        [Pure]
        public override IntRange GetActiveRange(ILayoutCollection collection, Rect viewportRect, Vector2 availableSize)
        {
            var visibleRange = GetApproximateVisibleRange(collection, viewportRect);
            return GetPreloadRange(visibleRange, collection.Count);
        }

        [Pure]
        private IntRange GetApproximateVisibleRange(ILayoutCollection collection, Rect viewportRect)
        {
            var (visibleIndexRange, visibleRectRange) = GetVisibleRange(collection, viewportRect);
            if (visibleIndexRange.IsEmpty)
                return GetRangeRelativeToLoadedRange(viewportRect);

            visibleIndexRange = GetRangeRoundedToWholeRows(visibleIndexRange);
            visibleIndexRange = GetVisibleRangeWithHypotheticalRows(viewportRect, visibleRectRange, visibleIndexRange);

            return visibleIndexRange;
        }

        [Pure]
        private IntRange GetRangeRelativeToLoadedRange(Rect viewportRect)
        {
            int itemCount = (viewportRect.size.y / (TypicalSizeAlong + Spacing.y)).CeilingInt() * CountAcross;

            var loadedIndices = _ranges.LoadedIndices;
            var layoutedRect = _ranges.LayoutedRect;
            if (loadedIndices.IsEmpty)
            {
                loadedIndices = _ranges.LastLayoutedIndices;
                layoutedRect = _ranges.LastLayoutedRect;

                if (loadedIndices.IsEmpty)
                    return new IntRange(0, itemCount);
            }

            bool isViewportBeforeLoadedRange = viewportRect.min.y < layoutedRect.min.y;

            loadedIndices = GetRangeRoundedToWholeRows(loadedIndices);

            var start = isViewportBeforeLoadedRange
                            ? (loadedIndices.Start - itemCount).AtLeast(0)
                            : loadedIndices.End;
            return new IntRange(start, start + itemCount);
        }

        [Pure]
        private (IntRange, Rect) GetVisibleRange(ILayoutCollection collection, Rect viewportRect)
        {
            var visibleIndexRange = IntRange.Empty;
            var visibleRectRange = Rect.zero;

            foreach (var i in _ranges.LayoutedIndices)
            {
                if (!collection.IsLayouted(i))
                    continue;

                var rect = collection.GetRect(i);

                if (rect.Overlaps(viewportRect))
                {
                    visibleIndexRange = visibleIndexRange.Union(i);
                    visibleRectRange = visibleRectRange.Union(rect);
                }
            }

            return (visibleIndexRange, visibleRectRange);
        }

        [Pure]
        private IntRange GetVisibleRangeWithHypotheticalRows(Rect viewportRect, Rect rectRange, IntRange indexRange)
        {
            var (spaceBefore, spaceAfter) = GetSpaceBeforeAndAfter(viewportRect, rectRange);
            var typicalSize = TypicalSizeAlong + Spacing.y;
            int rowsBefore = (spaceBefore.AtLeast(0) / typicalSize).CeilingInt();
            int rowsAfter = (spaceAfter.AtLeast(0) / typicalSize).CeilingInt();

            return new IntRange(
                (indexRange.Start - rowsBefore * CountAcross).AtLeast(0),
                indexRange.End + rowsAfter * CountAcross);
        }

        [Pure]
        private (float spaceBefore, float spaceAfter) GetSpaceBeforeAndAfter(Rect viewportRect, Rect rectRange) =>
            (rectRange.min.y - viewportRect.min.y, viewportRect.max.y - rectRange.max.y);

        [Pure]
        private IntRange GetPreloadRange(IntRange visibleIndexRange, int count)
        {
            // TODO: Deal with this special case
            if (visibleIndexRange.IsEmpty)
                return IntRange.Empty;

            visibleIndexRange = GetRangeRoundedToWholeRows(visibleIndexRange);

            return new IntRange(
                (visibleIndexRange.Start - CountAcross * PreloadCountAlong).AtLeast(0),
                (visibleIndexRange.End + CountAcross * PreloadCountAlong).AtMost(count));
        }

        #endregion
    }
}