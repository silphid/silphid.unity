using UnityEngine;

namespace Silphid.Showzup.ListLayouts
{
    public interface IListLayout
    {
        Rect GetItemRect(int index, Vector2 viewportSize);
        Vector2 GetContainerSize(int count, Vector2 viewportSize);
        IndexRange GetVisibleIndexRange(Rect rect);
    }
}