using Silphid.DataTypes;
using UnityEngine;

namespace Silphid.Showzup.Virtual.Layout
{
    public class DelegatingLayout : ILayout
    {
        private readonly LayoutInfo _info;
        private readonly ILayout _fixedLayout;
        private readonly ILayout _variableLayout;
        private readonly ILayout _keepAspectLayout;

        public DelegatingLayout(LayoutInfo info, RangeCache ranges)
        {
            _info = info;
            _fixedLayout = new FixedLayout(info, ranges);
            _variableLayout = new VariableLayout(info, ranges);
            _keepAspectLayout = new KeepAspectLayout(info, ranges);
        }

        public (float requiredHeight, float adjustment) Perform(LayoutDirection direction,
                                                                ILayoutCollection collection,
                                                                Rect viewportRect,
                                                                Vector2 availableSize) =>
            GetCurrentLayout()
               .Perform(direction, collection, viewportRect, availableSize);

        private ILayout GetCurrentLayout() =>
            _info.SizingAlong == SizingAlong.Fixed
                ? _fixedLayout
                : _info.SizingAlong == SizingAlong.Variable
                    ? _variableLayout
                    : _keepAspectLayout;

        public IntRange GetActiveRange(ILayoutCollection collection, Rect viewportRect, Vector2 availableSize) =>
            GetCurrentLayout()
               .GetActiveRange(collection, viewportRect, availableSize);
    }
}