using System;
using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class Vector3Extensions
    {
        #region Maths

        [Pure]
        public static float Dot(this Vector3 This, Vector3 other) => Vector3.Dot(This, other);

        [Pure]
        public static Vector3 Negate(this Vector3 This) => -This;

        [Pure]
        public static Vector3 Cross(this Vector3 This, Vector3 other) => Vector3.Cross(This, other);

        [Pure]
        public static Vector3 Subtract(this Vector3 This, Vector3 other) =>
            new Vector3(This.x - other.x, This.y - other.y, This.z - other.z);

        [Pure]
        public static Vector3 Multiply(this Vector3 This, Vector3 other) =>
            new Vector3(This.x * other.x, This.y * other.y, This.z * other.z);

        [Pure]
        public static Vector3 Divide(this Vector3 This, Vector3 other) =>
            new Vector3(This.x / other.x, This.y / other.y, This.z / other.z);

        [Pure]
        public static float Distance(this Vector3 This, Vector3 other) => Vector3.Distance(This, other);

        [Pure]
        public static Vector3 Average(this Vector3 This, Vector3 other) => (This + other) / 2;

        #endregion

        #region Aggregation

        [Pure]
        public static Vector3 Average(this IEnumerable<Vector3> This)
        {
            var sum = Vector3.zero;
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
        /// Returns interpolated value at given ratio, between This and target values.
        /// </summary>
        [Pure]
        [Obsolete("Use ratio.Lerp(source, target) instead")]
        public static Vector3 InterpolateTo(this Vector3 This, Vector3 target, float ratio) =>
            This + (target - This) * ratio;

        /// <summary>
        /// Uses This value as ratio to linearly interpolate between source and target.
        /// </summary>
        [Pure]
        public static Vector3 Lerp(this float This, Vector3 source, Vector3 target) =>
            source + (target - source) * This;

        /// <summary>
        /// Uses This value as ratio and a single control handle to interpolate between source and target (quadratic Bézier).
        /// </summary>
        [Pure]
        public static Vector3 Bezier(this float This, Vector3 source, Vector3 target, Vector3 handle) =>
            This.Lerp(This.Lerp(source, handle), This.Lerp(handle, target));

        /// <summary>
        /// Uses This value as ratio and two control handles to interpolate between source and target (cubic Bézier).
        /// </summary>
        [Pure]
        public static Vector3 Bezier(this float This, Vector3 source, Vector3 target, Vector3 sourceHandle, Vector3 targetHandle)
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
        public static Vector3 Smooth(this Vector3 This, Vector3 previousValue, float smoothness = Smoothness.Default) =>
            smoothness.Lerp(This, previousValue);

        #endregion

        #region Comparison

        [Pure]
        public static bool IsAlmostZero(this Vector3 This) =>
            This.x.IsAlmostZero() &&
            This.y.IsAlmostZero() &&
            This.z.IsAlmostZero();

        [Pure]
        public static bool IsAlmostEqualTo(this Vector3 This, Vector3 other) =>
            This.x.IsAlmostEqualTo(other.x) &&
            This.y.IsAlmostEqualTo(other.y) &&
            This.z.IsAlmostEqualTo(other.z);

        [Pure]
        public static bool IsAlmostEqualTo(this Vector3 This, Vector3 other, float epsilon) =>
            This.x.IsAlmostEqualTo(other.x, epsilon) &&
            This.y.IsAlmostEqualTo(other.y, epsilon) &&
            This.z.IsAlmostEqualTo(other.z, epsilon);

        [Pure]
        public static bool IsWithin(this Vector3 This, Bounds bounds) =>
            This.x >= bounds.min.x && This.x <= bounds.max.x &&
            This.y >= bounds.min.y && This.y <= bounds.max.y &&
            This.z >= bounds.min.z && This.z <= bounds.max.z;

        [Pure]
        public static bool IsWithin(this Vector3 This, Vector3 min, Vector3 max) =>
            This.x >= min.x && This.x <= max.x &&
            This.y >= min.y && This.y <= max.y &&
            This.z >= min.z && This.z <= max.z;

        #endregion

        #region Rounding

        [Pure]
        public static Vector3 Round(this Vector3 This) =>
            new Vector3(
                This.x.Round(),
                This.y.Round(),
                This.z.Round());

        [Pure]
        public static Vector3 RoundToInt(this Vector3 This) =>
            new Vector3(
                This.x.RoundToInt(),
                This.y.RoundToInt(),
                This.z.RoundToInt());

        [Pure]
        public static Vector3 RoundToInterval(this Vector3 This, float interval) =>
            new Vector3(
                This.x.RoundToInterval(interval),
                This.y.RoundToInterval(interval),
                This.z.RoundToInterval(interval));

        #endregion

        #region Component overriding

        [Pure]
        public static Vector3 WithX(this Vector3 This, float x) => new Vector3(x, This.y, This.z);

        [Pure]
        public static Vector3 WithY(this Vector3 This, float y) => new Vector3(This.x, y, This.z);

        [Pure]
        public static Vector3 WithZ(this Vector3 This, float z) => new Vector3(This.x, This.y, z);

        #endregion

        #region Clamping

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        [Pure]
        public static Vector3 Clamp(this Vector3 This, Vector3 min, Vector3 max) =>
            new Vector3(
                This.x.Clamp(min.x, max.x),
                This.y.Clamp(min.y, max.y),
                This.z.Clamp(min.z, max.z));

        [Pure]
        public static Vector3 Clamp(this Vector3 This, Bounds bounds) =>
            This.Clamp(bounds.min, bounds.max);

        /// <summary>
        /// Returns value, either within the [min, max] interval or otherwise
        /// applying a certain percentage of elasticity to the excess.
        /// <param name="elasticity">0f is like normal clamping, 1f is like no clamping.</param>
        /// </summary>
        [Pure]
        public static Vector3 ElasticClamp(this Vector3 This, Vector3 min, Vector3 max, float elasticity) =>
            new Vector3(
                This.x.ElasticClamp(min.x, max.x, elasticity),
                This.y.ElasticClamp(min.y, max.y, elasticity),
                This.z.ElasticClamp(min.z, max.z, elasticity));

        /// <summary>
        /// Returns value, either within the [min, max] interval or otherwise
        /// applying a certain percentage of elasticity to the excess.
        /// <param name="elasticity">0f is like normal clamping, 1f is like no clamping.</param>
        /// </summary>
        [Pure]
        public static Vector3 ElasticClamp(this Vector3 This, Bounds bounds, float elasticity) =>
            This.ElasticClamp(bounds.min, bounds.max, elasticity);

        #endregion

        #region Conversion

        [Pure]
        public static Vector2 ToVector2(this Vector3 This) =>
            new Vector2(This.x, This.y);

        #endregion
    }
}