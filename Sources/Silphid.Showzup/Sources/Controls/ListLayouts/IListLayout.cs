using UnityEngine;

namespace Silphid.Showzup.ListLayouts
{
    public interface IListLayout
    {
        Rect GetItemRect(int index);
        Vector2 GetContainerSize(int count);
        IndexRange GetVisibleIndexRange(Rect rect);
    }
}