using System;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class IObservableVector3Extensions
    {
        #region Maths

        [Pure]
        public static IObservable<float> Dot(this IObservable<Vector3> This, Vector3 other) =>
            This.Select(x => x.Dot(other));

        [Pure]
        public static IObservable<Vector3> Negate(this IObservable<Vector3> This) =>
            This.Select(x => x.Negate());

        [Pure]
        public static IObservable<Vector3> Cross(this IObservable<Vector3> This, Vector3 other) =>
            This.Select(x => x.Cross(other));

        [Pure]
        public static IObservable<Vector3> Subtract(this IObservable<Vector3> This, Vector3 other) =>
            This.Select(x => x.Subtract(other));

        [Pure]
        public static IObservable<Vector3> Multiply(this IObservable<Vector3> This, Vector3 other) =>
            This.Select(x => x.Multiply(other));

        [Pure]
        public static IObservable<float> Distance(this IObservable<Vector3> This, Vector3 other) =>
            This.Select(x => x.Distance(other));

        [Pure]
        public static IObservable<float> Magnitude(this IObservable<Vector3> This) =>
            This.Select(x => x.magnitude);

        [Pure]
        public static IObservable<Vector3> Average(this IObservable<Vector3> This, Vector3 other) =>
            This.Select(x => x.Average(other));

        #endregion

        #region Interpolation

        /// <summary>
        /// Uses This observable values as ratio to interpolate between source and target.
        /// </summary>
        public static IObservable<Vector3> Lerp(this IObservable<float> This, Vector3 source, Vector3 target) =>
            This.Select(x => x.Lerp(source, target));

        /// <summary>
        /// Uses This value as ratio and a single control handle to interpolate between source and target (quadratic Bézier).
        /// </summary>
        [Pure]
        public static IObservable<Vector3> Bezier(this IObservable<float> This, Vector3 source, Vector3 target, Vector3 handle) =>
            This.Select(x => x.Bezier(source, target, handle));

        /// <summary>
        /// Uses This value as ratio and two control handles to interpolate between source and target (cubic Bézier).
        /// </summary>
        [Pure]
        public static IObservable<Vector3> Bezier(this IObservable<float> This, Vector3 source, Vector3 target, Vector3 sourceHandle, Vector3 targetHandle) =>
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
        public static IObservable<Vector3> Smooth(this IObservable<Vector3> This, Vector3 initialValue, float smoothness) =>
            This.Aggregate(initialValue, (acc, value) => value.Smooth(acc, smoothness));

        #endregion

        #region Comparison

        [Pure]
        public static IObservable<bool> IsAlmostZero(this IObservable<Vector3> This) =>
            This.Select(x => x.IsAlmostZero());

        [Pure]
        public static IObservable<bool> IsAlmostEqualTo(this IObservable<Vector3> This, Vector3 other) =>
            This.Select(x => x.IsAlmostEqualTo(other));

        [Pure]
        public static IObservable<bool> IsAlmostEqualTo(this IObservable<Vector3> This, Vector3 other, float epsilon) =>
            This.Select(x => x.IsAlmostEqualTo(other, epsilon));

        [Pure]
        public static IObservable<bool> IsWithin(this IObservable<Vector3> This, Bounds bounds) =>
            This.Select(x => x.IsWithin(bounds));

        [Pure]
        public static IObservable<bool> IsWithin(this IObservable<Vector3> This, Vector3 min, Vector3 max) =>
            This.Select(x => x.IsWithin(min, max));

        #endregion

        #region Rounding

        [Pure]
        public static IObservable<Vector3> Round(this IObservable<Vector3> This) =>
            This.Select(x => x.Round());

        [Pure]
        public static IObservable<Vector3> RoundToInt(this IObservable<Vector3> This) =>
            This.Select(x => x.RoundToInt());

        [Pure]
        public static IObservable<Vector3> RoundToInterval(this IObservable<Vector3> This, float interval) =>
            This.Select(x => x.RoundToInterval(interval));

        #endregion

        #region Component overriding

        [Pure]
        public static IObservable<Vector3> WithX(this IObservable<Vector3> This, float newX) =>
            This.Select(x => x.WithX(newX));

        [Pure]
        public static IObservable<Vector3> WithY(this IObservable<Vector3> This, float newY) =>
            This.Select(x => x.WithY(newY));

        [Pure]
        public static IObservable<Vector3> WithZ(this IObservable<Vector3> This, float newZ) =>
            This.Select(x => x.WithZ(newZ));

        #endregion

        #region Clamping

        [Pure]
        public static IObservable<Vector3> Clamp(this IObservable<Vector3> This, Bounds bounds) =>
            This.Select(x => x.Clamp(bounds));

        [Pure]
        public static IObservable<Vector3> Clamp(this IObservable<Vector3> This, Vector3 min, Vector3 max) =>
            This.Select(x => x.Clamp(min, max));

        /// <summary>
        /// Returns value, either within the [min, max] interval or otherwise
        /// applying a certain percentage of elasticity to the excess.
        /// <param name="elasticity">0f is like normal clamping, 1f is like no clamping.</param>
        /// </summary>
        [Pure]
        public static IObservable<Vector3> ElasticClamp(this IObservable<Vector3> This, Vector3 min, Vector3 max, float elasticity) =>
            This.Select(x => x.ElasticClamp(min, max, elasticity));

        /// <summary>
        /// Returns value, either within the [min, max] interval or otherwise
        /// applying a certain percentage of elasticity to the excess.
        /// <param name="elasticity">0f is like normal clamping, 1f is like no clamping.</param>
        /// </summary>
        [Pure]
        public static IObservable<Vector3> ElasticClamp(this IObservable<Vector3> This, Bounds bounds, float elasticity) =>
            This.Select(x => x.ElasticClamp(bounds, elasticity));

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
        public static IObservable<Vector3> Velocity(this IObservable<Vector3> This, float smoothness = Smoothness.Responsive, IObservable<Vector3> rawVelocities = null, Func<float> getTime = null) =>
            new Vector3VelocityObservable(This, smoothness, rawVelocities, getTime);

        #endregion

        #region Conversion

        [Pure]
        public static IObservable<Vector2> ToVector2(this IObservable<Vector3> This) =>
            This.Select(x => x.ToVector2());

        #endregion
    }
}