using UnityEngine;

namespace Silphid.Showzup.Components
{
    public abstract class ListLayout : MonoBehaviour, IListLayout
    {
        public Vector2 ItemSize;
        public Vector2 ItemPadding;
        public Vector2 ItemSpacing;
        public Vector2 ContainerPadding;

        private IListLayout _layout;
        private IListLayout Layout => _layout ?? (_layout = CreateLayout());

        protected abstract IListLayout CreateLayout();
        
        public Rect GetItemRect(int index) =>
            Layout.GetItemRect(index);
        
        public Vector2 GetContainerSize(int count) =>
            Layout.GetContainerSize(count);
    }
}