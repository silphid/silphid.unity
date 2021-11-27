using JetBrains.Annotations;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class RectExtensions
    {
        public static Rect Translate(this Rect This, Vector2 vector) =>
            new Rect(This.xMin + vector.x, This.yMin + vector.y, This.width, This.height);

        public static Rect FlippedY(this Rect This) =>
            new Rect(This.x, -(This.y + This.height), This.width, This.height);

        public static Rect FlippedXY(this Rect This) =>
            new Rect(This.y, This.x, This.height, This.width);

        public static Rect Union(this Rect This, Rect other) =>
            This.IsEmpty()
                ? other
                : other.IsEmpty()
                    ? This
                    : Rect.MinMaxRect(
                        This.xMin.Min(other.xMin),
                        This.yMin.Min(other.yMin),
                        This.xMax.Max(other.xMax),
                        This.yMax.Max(other.yMax));

        #region Comparison

        public static bool IsEmpty(this Rect This) =>
            This.xMin.IsAlmostEqualTo(This.xMax) || This.yMin.IsAlmostEqualTo(This.yMax);

        [Pure]
        public static bool IsAlmostEqualTo(this Rect This, Rect other) =>
            This.min.IsAlmostEqualTo(other.min) && This.size.IsAlmostEqualTo(other.size);

        [Pure]
        public static bool IsAlmostEqualTo(this Rect This, Rect other, float epsilon) =>
            This.min.IsAlmostEqualTo(other.min, epsilon) && This.size.IsAlmostEqualTo(other.size, epsilon);

        #endregion
    }
}