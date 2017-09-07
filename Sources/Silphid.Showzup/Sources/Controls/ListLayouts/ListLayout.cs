using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.ListLayouts
{
    public abstract class ListLayout : IListLayout
    {
        protected readonly Vector2 ItemSize;
        protected readonly Vector2 Spacing;
        protected readonly RectOffset Padding;

        protected ListLayout(Vector2 itemSize, Vector2 spacing, RectOffset padding)
        {
            ItemSize = itemSize;
            Spacing = spacing;
            Padding = padding;
        }
        
        protected Vector2 FirstItemPosition =>
            new Vector2(Padding.left, -Padding.top);

        protected Vector2 ItemOffset =>
            (ItemSize + Spacing).NegateY();

        public abstract Rect GetItemRect(int index);
        public abstract Vector2 GetContainerSize(int count);
        public abstract IndexRange GetVisibleIndexRange(Rect rect);
    }
}