using Silphid.DataTypes;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.Virtual.Layout
{
    public class FixedLayout : LayoutBase
    {
        public FixedLayout(ILayoutInfo info, RangeCache ranges)
            : base(info, ranges) {}

        protected virtual float GetHeight(Vector2 availableSize) => Size.y;

        protected override float? GetRowHeight(int start, int end, ILayoutCollection collection, Vector2 availableSize) =>
            GetHeight(availableSize);

        protected override float GetStartPosY(int startIndex,
                                              LayoutDirection direction,
                                              Rect viewportRect,
                                              Vector2 availableSize)
        {
            // TODO: If direction Backward, use endIndex instead!(??) *OR* refactor LayoutBase for forward only when FixedLayout
            var rowsBefore = startIndex / CountAcross;
            var height = GetHeight(availableSize);
            
            return MinMargin.y +
                   rowsBefore * (height + Spacing.y) +
                   (direction == LayoutDirection.Forward
                        ? 0
                        : height);
        }

        protected override (float requiredHeight, float adjustment) OnLayoutCompleted(LayoutDirection direction,
                                                                                      ILayoutCollection collection,
                                                                                      bool isTopLoaded,
                                                                                      bool isBottomLoaded,
                                                                                      float startPosY,
                                                                                      float lastPosY,
                                                                                      Vector2 availableSize)
        {
            var rows = collection.Count.CeilingDivisionBy(CountAcross);
            var requiredHeight = MinMargin.y + rows * GetHeight(availableSize) + (rows - 1) * Spacing.y + MaxMargin.y;
            return (requiredHeight, 0);
        }

        public override IntRange GetActiveRange(ILayoutCollection collection, Rect viewportRect, Vector2 availableSize)
        {
            var viewportRange = new FloatRange(viewportRect.yMin, viewportRect.yMax);
            var margin = MinMargin.y;
            var rowSize = GetHeight(availableSize) + Spacing.y;
            int rowsToStart = ((viewportRange.Start - margin).AtLeast(0) / rowSize).FloorInt();
            int rowsToEnd = ((viewportRange.End - margin).AtLeast(0) / rowSize).CeilingInt();

            return new IntRange(
                ((rowsToStart - PreloadCountAlong) * CountAcross).AtLeast(0),
                ((rowsToEnd + PreloadCountAlong) * CountAcross).AtMost(collection.Count));
        }
    }
}