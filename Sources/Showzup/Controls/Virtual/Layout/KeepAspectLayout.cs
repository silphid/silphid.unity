using UnityEngine;

namespace Silphid.Showzup.Virtual.Layout
{
    public class KeepAspectLayout : FixedLayout
    {
        public KeepAspectLayout(ILayoutInfo info, RangeCache ranges)
            : base(info, ranges) {}

        protected override float GetHeight(Vector2 availableSize)
        {
            var allowedWidth = GetAllowedWidth(availableSize.x);
            var cellWidth = GetCellWidth(allowedWidth);

            return cellWidth / Ratio;
        }
    }
}