using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class Vector2Extensions
    {
        #region Maths

        [Pure]
        public static float Dot(this Vector2 This, Vector2 other) => Vector2.Dot(This, other);

        [Pure]
        public static Vector2 Negate(this Vector2 This) => -This;

        [Pure]
        public static Vector2 NegateX(this Vector2 This) => new Vector2(-This.x, This.y);

        [Pure]
        public static Vector2 NegateY(this Vector2 This) => new Vector2(This.x, -This.y);

        [Pure]
        public static Vector2 Subtract(this Vector2 This, Vector2 other) =>
            new Vector2(This.x - other.x, This.y - other.y);

        [Pure]
        public static Vector2 Multiply(this Vector2 This, Vector2 other) =>
            new Vector2(This.x * other.x, This.y * other.y);

        [Pure]
        public static Vector2 Divide(this Vector2 This, Vector2 other) =>
            new Vector2(This.x / other.x, This.y / other.y);

        [Pure]
        public static float Delta(this Vector2 This, Vector2 other) => Vector2.Distance(This, other);

        [Pure]
        public static Vector2 Average(this Vector2 This, Vector2 other) => (This + other) / 2;

        #endregion

        #region Aggregation

        [Pure]
        public static Vector2 Average(this IEnumerable<Vector2> This)
        {
            var sum = Vector2.zero;
            int count = 0;
            foreach (var vector in This)
            {
                sum += vector;
                count++;
            }
            return sum / count;
        }

        #endregion

        #region Interpolation

        /// <summary>
        /// Uses This value as ratio to interpolate between source and target.
        /// </summary>
        [Pure]
        public static Vector2 Lerp(this float This, Vector2 source, Vector2 target) =>
            source + (target - source) * This;

        /// <summary>
        /// Uses This value as ratio and a single control handle to interpolate between source and target (quadratic Bézier).
        /// </summary>
        [Pure]
        public static Vector2 Bezier(this float This, Vector2 source, Vector2 target, Vector2 handle) =>
            This.Lerp(This.Lerp(source, handle), This.Lerp(handle, target));

        /// <summary>
        /// Uses This value as ratio and two control handles to interpolate between source and target (cubic Bézier).
        /// </summary>
        [Pure]
        public static Vector2 Bezier(this float This, Vector2 source, Vector2 target, Vector2 sourceHandle, Vector2 targetHandle)
        {
            var a = This.Lerp(source, sourceHandle);
            var b = This.Lerp(sourceHandle, targetHandle);
            var c = This.Lerp(targetHandle, target);
            return This.Lerp(This.Lerp(a, b), This.Lerp(b, c));
        }

        /// <summary>
        /// Smooths this (new) value compared to its previous value to reduce noise or sudden peaks.
        /// Note that smoothness is affected by the rate at which this method is invoked and should be adjusted
        /// accordingly.
        /// </summary>
        /// <param name="This">The new value to be smoothed.</param>
        /// <param name="previousValue">The previous value to smooth relatively from.</param>
        /// <param name="smoothness">A number between 0 (no smoothing) and 1 (ignores new values).</param>
        [Pure]
        public static Vector2 Smooth(this Vector2 This, Vector2 previousValue, float smoothness) =>
            smoothness.Lerp(This, previousValue);

        #endregion

        #region Comparison

        [Pure]
        public static bool IsAlmostZero(this Vector2 This) =>
            This.x.IsAlmostZero() &&
            This.y.IsAlmostZero();

        [Pure]
        public static bool IsAlmostEqualTo(this Vector2 This, Vector2 other) =>
            This.x.IsAlmostEqualTo(other.x) &&
            This.y.IsAlmostEqualTo(other.y);

        [Pure]
        public static bool IsAlmostEqualTo(this Vector2 This, Vector2 other, float epsilon) =>
            This.x.IsAlmostEqualTo(other.x, epsilon) &&
            This.y.IsAlmostEqualTo(other.y, epsilon);

        [Pure]
        public static bool IsWithin(this Vector2 This, Rect rect) =>
            This.x >= rect.xMin && This.x <= rect.xMax &&
            This.y >= rect.yMin && This.y <= rect.yMax;

        [Pure]
        public static bool IsWithin(this Vector2 This, Vector2 min, Vector2 max) =>
            This.x >= min.x && This.x <= max.x &&
            This.y >= min.y && This.y <= max.y;

        #endregion

        #region Rounding

        [Pure]
        public static Vector2 Round(this Vector2 This) =>
            new Vector2(
                This.x.Round(),
                This.y.Round());

        [Pure]
        public static Vector2 RoundToInt(this Vector2 This) =>
            new Vector2(
                This.x.RoundToInt(),
                This.y.RoundToInt());

        [Pure]
        public static Vector2 RoundToInterval(this Vector2 This, float increment) =>
            new Vector2(
                This.x.RoundToInterval(increment),
                This.y.RoundToInterval(increment));

        #endregion

        #region Component overriding

        [Pure]
        public static Vector2 WithX(this Vector2 This, float x) => new Vector2(x, This.y);

        [Pure]
        public static Vector2 WithY(this Vector2 This, float y) => new Vector2(This.x, y);

        #endregion

        #region Clamping

        [Pure]
        public static Vector2 Clamp(this Vector2 This, Rect rect) =>
            new Vector2(This.x.Clamp(rect.xMin, rect.xMax), This.y.Clamp(rect.yMin, rect.yMax));

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        [Pure]
        public static Vector2 Clamp(this Vector2 This, Vector2 min, Vector2 max) =>
            new Vector2(This.x.Clamp(min.x, max.x), This.y.Clamp(min.y, max.y));

        /// <summary>
        /// Returns value, either within the [min, max] interval or otherwise
        /// applying a certain percentage of elasticity to the excess.
        /// <param name="elasticity">0f is like normal clamping, 1f is like no clamping.</param>
        /// </summary>
        [Pure]
        public static Vector2 ElasticClamp(this Vector2 This, Vector2 min, Vector2 max, float elasticity) =>
            new Vector2(
                This.x.ElasticClamp(min.x, max.x, elasticity),
                This.y.ElasticClamp(min.y, max.y, elasticity));

        /// <summary>
        /// Returns value, either within the [min, max] interval or otherwise
        /// applying a certain percentage of elasticity to the excess.
        /// <param name="elasticity">0f is like normal clamping, 1f is like no clamping.</param>
        /// </summary>
        [Pure]
        public static Vector2 ElasticClamp(this Vector2 This, Rect rect, float elasticity) =>
            This.ElasticClamp(rect.min, rect.max, elasticity);

        #endregion

        #region Conversion

        [Pure]
        public static Vector3 ToVector3(this Vector2 This, float z = 0) =>
            new Vector3(This.x, This.y, z);

        #endregion
    }
}