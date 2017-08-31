using UnityEngine;

namespace Silphid.Showzup
{
    public interface IListLayout
    {
        Rect GetItemRect(int index);
        Vector2 GetContainerSize(int count);
    }
}