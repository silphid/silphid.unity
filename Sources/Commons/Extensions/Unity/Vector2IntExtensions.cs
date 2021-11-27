using JetBrains.Annotations;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class Vector2IntExtensions
    {
        #region Maths

        [Pure]
        public static Vector2Int Negate(this Vector2Int This) => new Vector2Int(-This.x, -This.y);

        [Pure]
        public static Vector2Int NegateX(this Vector2Int This) => new Vector2Int(-This.x, This.y);

        [Pure]
        public static Vector2Int NegateY(this Vector2Int This) => new Vector2Int(This.x, -This.y);

        [Pure]
        public static Vector2Int Subtract(this Vector2Int This, Vector2Int other) =>
            new Vector2Int(This.x - other.x, This.y - other.y);

        [Pure]
        public static Vector2Int Multiply(this Vector2Int This, Vector2Int other) =>
            new Vector2Int(This.x * other.x, This.y * other.y);

        [Pure]
        public static Vector2Int Divide(this Vector2Int This, Vector2Int other) =>
            new Vector2Int(This.x / other.x, This.y / other.y);

        [Pure]
        public static float Delta(this Vector2Int This, Vector2Int other) => Vector2Int.Distance(This, other);

        #endregion

        #region Comparison

        [Pure]
        public static bool IsWithin(this Vector2Int This, Rect rect) =>
            This.x >= rect.xMin && This.x <= rect.xMax && This.y >= rect.yMin && This.y <= rect.yMax;

        [Pure]
        public static bool IsWithin(this Vector2Int This, Vector2Int min, Vector2Int max) =>
            This.x >= min.x && This.x <= max.x && This.y >= min.y && This.y <= max.y;

        #endregion

        #region Components

        [Pure]
        public static Vector2Int WithX(this Vector2Int This, int x) => new Vector2Int(x, This.y);

        [Pure]
        public static Vector2Int WithY(this Vector2Int This, int y) => new Vector2Int(This.x, y);

        [Pure]
        public static Vector2Int FlippedXY(this Vector2Int This) => new Vector2Int(This.y, This.x);

        #endregion

        #region Clamping

        /// <summary>
        /// Returns the minimum value between this and another value
        /// </summary>
        public static Vector2Int Min(this Vector2Int This, Vector2Int min) =>
            new Vector2Int(This.x.Min(min.x), This.y.Min(min.y));

        /// <summary>
        /// Returns the maximum value between this and another value
        /// </summary>
        public static Vector2Int Max(this Vector2Int This, Vector2Int max) =>
            new Vector2Int(This.x.Max(max.x), This.y.Max(max.y));

        /// <summary>
        /// Returns value clamped to be greater than or at least equal to given value
        /// </summary>
        public static Vector2Int AtLeast(this Vector2Int This, Vector2Int min) =>
            Max(This, min);

        /// <summary>
        /// Returns value clamped to be at less than or at most equal to given value
        /// </summary>
        public static Vector2Int AtMost(this Vector2Int This, Vector2Int max) =>
            Min(This, max);

        [Pure]
        public static Vector2Int Clamp(this Vector2Int This, RectInt rect) =>
            new Vector2Int(This.x.Clamp(rect.xMin, rect.xMax), This.y.Clamp(rect.yMin, rect.yMax));

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        [Pure]
        public static Vector2Int Clamp(this Vector2Int This, Vector2Int min, Vector2Int max) =>
            new Vector2Int(This.x.Clamp(min.x, max.x), This.y.Clamp(min.y, max.y));

        #endregion
    }
}