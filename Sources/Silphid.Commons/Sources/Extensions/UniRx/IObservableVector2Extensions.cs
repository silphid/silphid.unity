using System;
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
        public static IObservable<float> Distance(this IObservable<Vector2> This, Vector2 other) =>
            This.Select(x => x.Distance(other));

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

        /// <summary>
        /// Returns value, either within the [min, max] interval or otherwise
        /// applying a certain percentage of elasticity to the excess.
        /// <param name="elasticity">0f is like normal clamping, 1f is like no clamping.</param>
        /// </summary>
        [Pure]
        public static IObservable<Vector2> ElasticClamp(this IObservable<Vector2> This, Vector2 min, Vector2 max, float elasticity) =>
            This.Select(x => x.ElasticClamp(min, max, elasticity));

        /// <summary>
        /// Returns value, either within the [min, max] interval or otherwise
        /// applying a certain percentage of elasticity to the excess.
        /// <param name="elasticity">0f is like normal clamping, 1f is like no clamping.</param>
        /// </summary>
        [Pure]
        public static IObservable<Vector2> ElasticClamp(this IObservable<Vector2> This, Rect rect, float elasticity) =>
            This.Select(x => x.ElasticClamp(rect, elasticity));

        #endregion

        #region Velocity

        /// <summary>
        /// Returns an observable that outputs the varying velocity of input values, every time a new input value is
        /// received, while smoothing that velocity to avoid peaks or abnormalities.
        /// <param name="This">Input values to calculate velocity of.</param>
        /// <param name="smoothness">Amount of smoothness to apply to output velocities. A float from 0f (none) to some
        /// value less than 1f (or some predefined value from the Smoothness class).</param>
        /// <param name="rawVelocities">Optional input velocities that can be used to instantly reset current
        /// velocity to a specific value and output it as is. If that observable completes or errors, the output observable
        /// will also complete or error.</param>
        /// <param name="getTime">Optional time selector that can be used to provide an arbitrary time
        /// (ie: time-scale-independent, etc). Defaults to "() => Time.time"</param>
        /// </summary>
        [Pure]
        public static IObservable<Vector2> Velocity(this IObservable<Vector2> This, float smoothness = Smoothness.Responsive, IObservable<Vector2> rawVelocities = null, Func<float> getTime = null) =>
            new Vector2VelocityObservable(This, smoothness, rawVelocities, getTime);

        #endregion

        #region Conversion

        [Pure]
        public static IObservable<Vector3> ToVector3(this IObservable<Vector2> This) =>
            This.Select(x => x.ToVector3());

        #endregion
    }
}