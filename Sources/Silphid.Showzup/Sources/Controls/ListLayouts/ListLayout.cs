using UnityEngine;

namespace Silphid.Showzup
{
    public abstract class ListLayout : IListLayout
    {
        protected readonly Vector2 ItemPadding;
        protected readonly Vector2 ItemSize;
        protected readonly Vector2 ItemSpacing;
        protected readonly Vector2 ContainerPadding;

        protected ListLayout(Vector2 itemPadding, Vector2 itemSize, Vector2 itemSpacing, Vector2 containerPadding)
        {
            ItemPadding = itemPadding;
            ItemSize = itemSize;
            ItemSpacing = itemSpacing;
            ContainerPadding = containerPadding;
        }

        protected Vector2 FirstItemPosition =>
            ContainerPadding + ItemPadding;

        protected Vector2 ItemOffset =>
            ItemSize + ItemPadding + ItemSpacing;

        public abstract Rect GetItemRect(int index);
        public abstract Vector2 GetContainerSize(int count);
    }
}