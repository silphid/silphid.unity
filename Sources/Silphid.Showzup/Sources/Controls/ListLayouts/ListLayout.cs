using Silphid.DataTypes;
using UnityEngine;

namespace Silphid.Showzup.ListLayouts
{
    public abstract class ListLayout : MonoBehaviour, IListLayout
    {
        public RectOffset Padding;

        protected Vector2 FirstItemPosition =>
            new Vector2(Padding.left, Padding.top);
        
        public abstract Rect GetItemRect(int index, Vector2 viewportSize);
        public abstract Vector2 GetContainerSize(int count, Vector2 viewportSize);
        public abstract IntRange GetVisibleIndexRange(Rect rect);
    }
}