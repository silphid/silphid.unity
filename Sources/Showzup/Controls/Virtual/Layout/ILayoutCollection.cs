using UnityEngine;

namespace Silphid.Showzup.Virtual.Layout
{
    public interface ILayoutCollection
    {
        int Count { get; }
        bool IsLoaded(int index);
        bool IsLayouted(int index);
        Vector2 GetPreferredSize(int index);
        Rect GetRect(int index);
        void SetRect(int index, Rect rect);
    }
}