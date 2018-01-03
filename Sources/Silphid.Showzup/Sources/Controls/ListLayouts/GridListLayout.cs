using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.ListLayouts
{
    public abstract class GridListLayout : ListLayout
    {
        public Vector2 Spacing;
        public Vector2 ItemSize;

        protected Vector2 ItemOffset => ItemSize + Spacing;

        protected abstract Vector2 GetWrappedItemIndices(int index);
        protected abstract Vector2 GetItemSize(Vector2 viewportSize);

        public override Rect GetItemRect(int index, Vector2 viewportSize) =>
            new Rect(
                FirstItemPosition + ItemOffset.Multiply(GetWrappedItemIndices(index)),
                GetItemSize(viewportSize));
    }
}