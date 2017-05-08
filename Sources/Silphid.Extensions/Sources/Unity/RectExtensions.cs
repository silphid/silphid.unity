using UnityEngine;

namespace Silphid.Extensions
{
    public static class RectExtensions
    {
        public static Rect Translate(this Rect rect, Vector2 vector)
        {
            var r2 = rect;
            r2.xMin += vector.x;
            r2.yMin += vector.y;

            return r2;
        }
    }
}
