using UnityEngine;

namespace Silphid.Showzup.Virtual.Layout
{
    public static class RectTransformExtensions
    {
        public static void SetLayoutRect(this RectTransform This, Rect rect)
        {
            This.anchorMin = Vector2.up;
            This.anchorMax = Vector2.up;
            This.pivot = Vector2.zero;
            This.anchoredPosition = rect.min;
            This.sizeDelta = rect.size;
        }
        
        public static Rect GetLayoutRect(this RectTransform This) =>
            new Rect(This.anchoredPosition, This.sizeDelta);
    }
}