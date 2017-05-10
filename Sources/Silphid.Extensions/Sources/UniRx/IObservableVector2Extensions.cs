using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class IObservableVector2Extensions
    {
        #region Maths

        [Pure]
        public static IObservable<float> Dot(this IObservable<Vector2> This, Vector2 other) =>
            This.Select(x => x.Dot(other));

        [Pure]
        public static IObservable<Vector2> Negate(this IObservable<Vector2> This) =>
            This.Select(x => x.Negate());

        [Pure]
        public static IObservable<Vector2> Subtract(this IObservable<Vector2> This, Vector2 other) =>
            This.Select(x => x.Subtract(other));

        [Pure]
        public static IObservable<Vector2> Multiply(this IObservable<Vector2> This, Vector2 other) =>
            This.Select(x => x.Multiply(other));

        [Pure]
        public static IObservable<float> Delta(this IObservable<Vector2> This, Vector2 other) =>
            This.Select(x => x.Delta(other));

        [Pure]
        public static IObservable<float> Magnitude(this IObservable<Vector2> This) =>
            This.Select(x => x.magnitude);

        [Pure]
        public static IObservable<Vector2> Average(this IObservable<Vector2> This, Vector2 other) =>
            This.Select(x => x.Average(other));

        #endregion

        #region Interpolation

        /// <summary>
        /// Uses This observable values as ratio to interpolate between source and target.
        /// </summary>
        [Pure]
        public static IObservable<Vector2> Lerp(this IObservable<float> This, Vector2 source, Vector2 target) =>
            This.Select(x => x.Lerp(source, target));

        /// <summary>
        /// Uses This value as ratio and a single control handle to interpolate between source and target (quadratic Bézier).
        /// </summary>
        [Pure]
        public static IObservable<Vector2> Bezier(this IObservable<float> This, Vector2 source, Vector2 target, Vector2 handle) =>
            This.Select(x => x.Bezier(source, target, handle));

        /// <summary>
        /// Uses This value as ratio and two control handles to interpolate between source and target (cubic Bézier).
        /// </summary>
        [Pure]
        public static IObservable<Vector2> Bezier(this IObservable<float> This, Vector2 source, Vector2 target, Vector2 sourceHandle, Vector2 targetHandle) =>
            This.Select(x => x.Bezier(source, target, sourceHandle, targetHandle));

        /// <summary>
        /// Smooths this (new) value compared to its previous value to reduce noise or sudden peaks.
        /// Note that smoothness is affected by the rate at which this method is invoked and should be adjusted
        /// accordingly.
        /// </summary>
        /// <param name="This">The stream of values to be smoothed.</param>
        /// <param name="initialValue">The value to start smoothing at.</param>
        /// <param name="smoothness">A number between 0 (no smoothing) and 1 (ignores new values).</param>
        [Pure]
        public static IObservable<Vector2> Smooth(this IObservable<Vector2> This, Vector2 initialValue, float smoothness) =>
            This.Aggregate(initialValue, (acc, value) => value.Smooth(acc, smoothness));

        #endregion

        #region Comparison

        [Pure]
        public static IObservable<bool> IsAlmostZero(this IObservable<Vector2> This) =>
            This.Select(x => x.IsAlmostZero());

        [Pure]
        public static IObservable<bool> IsAlmostEqualTo(this IObservable<Vector2> This, Vector2 other) =>
            This.Select(x => x.IsAlmostEqualTo(other));

        [Pure]
        public static IObservable<bool> IsAlmostEqualTo(this IObservable<Vector2> This, Vector2 other, float epsilon) =>
            This.Select(x => x.IsAlmostEqualTo(other, epsilon));

        [Pure]
        public static IObservable<bool> IsWithin(this IObservable<Vector2> This, Rect rect) =>
            This.Select(x => x.IsWithin(rect));

        [Pure]
        public static IObservable<bool> IsWithin(this IObservable<Vector2> This, Vector2 min, Vector2 max) =>
            This.Select(x => x.IsWithin(min, max));

        #endregion

        #region Rounding

        [Pure]
        public static IObservable<Vector2> Round(this IObservable<Vector2> This) =>
            This.Select(x => x.Round());

        [Pure]
        public static IObservable<Vector2> RoundToInt(this IObservable<Vector2> This) =>
            This.Select(x => x.RoundToInt());

        [Pure]
        public static IObservable<Vector2> RoundToInterval(this IObservable<Vector2> This, float interval) =>
            This.Select(x => x.RoundToInterval(interval));

        #endregion

        #region Component overriding

        [Pure]
        public static IObservable<Vector2> WithX(this IObservable<Vector2> This, float newX) =>
            This.Select(x => x.WithX(newX));

        [Pure]
        public static IObservable<Vector2> WithY(this IObservable<Vector2> This, float newY) =>
            This.Select(x => x.WithY(newY));

        #endregion

        #region Clamping

        [Pure]
        public static IObservable<Vector2> Clamp(this IObservable<Vector2> This, Rect rect) =>
            This.Select(x => x.Clamp(rect));

        [Pure]
        public static IObservable<Vector2> Clamp(this IObservable<Vector2> This, Vector2 min, Vector2 max) =>
            This.Select(x => x.Clamp(min, max));

        #endregion

        #region Conversion

        [Pure]
        public static IObservable<Vector3> ToVector3(this IObservable<Vector2> This) =>
            This.Select(x => x.ToVector3());

        #endregion
    }
}