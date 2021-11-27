using System;
using JetBrains.Annotations;
using UniRx;

namespace Silphid.Extensions
{
    public static class IObservableFloatExtensions
    {
        #region Maths

        [Pure]
        public static IObservable<int> Sign(this IObservable<float> This) =>
            This.Select(x => x.Sign());

        [Pure]
        public static IObservable<float> Abs(this IObservable<float> This) =>
            This.Select(x => x.Abs());

        [Pure]
        public static IObservable<float> Floor(this IObservable<float> This) =>
            This.Select(x => x.Floor());

        [Pure]
        public static IObservable<float> Ceiling(this IObservable<float> This) =>
            This.Select(x => x.Ceiling());

        /// <summary>
        /// Returns fractional part of given value.
        /// </summary>
        [Pure]
        public static IObservable<float> Fractional(this IObservable<float> This) => This.Select(x => x.Fractional());

        /// <summary>
        /// Returns absolute delta between to values.
        /// </summary>
        [Pure]
        public static IObservable<float> Delta(this IObservable<float> This, float other) =>
            This.Select(x => x.Delta(other));

        [Pure]
        public static IObservable<float> Average(this IObservable<float> This, float other) =>
            This.Select(x => x.Average(other));

        #endregion

        #region Interpolation

        /// <summary>
        /// Uses This observable values as ratio to interpolate between source and target.
        /// </summary>
        [Pure]
        public static IObservable<float> Lerp(this IObservable<float> This, float source, float target) =>
            This.Select(x => x.Lerp(source, target));

        /// <summary>
        /// Uses This value as ratio and a single control handle to interpolate between source and target (quadratic Bézier).
        /// </summary>
        [Pure]
        public static IObservable<float>
            Bezier(this IObservable<float> This, float source, float target, float handle) =>
            This.Select(x => x.Bezier(source, target, handle));

        /// <summary>
        /// Uses This value as ratio and two control handles to interpolate between source and target (cubic Bézier).
        /// </summary>
        [Pure]
        public static IObservable<float> Bezier(this IObservable<float> This,
                                                float source,
                                                float target,
                                                float sourceHandle,
                                                float targetHandle) =>
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
        public static IObservable<float> Smooth(this IObservable<float> This, float initialValue, float smoothness) =>
            This.Aggregate(initialValue, (acc, value) => value.Smooth(acc, smoothness));

        #endregion

        #region Ratio

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval
        /// </summary>
        [Pure]
        public static IObservable<float> Ratio(this IObservable<float> This, float min, float max) =>
            This.Select(x => x.Ratio(min, max));

        #endregion

        #region Transposition

        /// <summary>
        /// Returns [minTo, maxTo] interpolation of given value within the [minFrom, maxFrom] interval
        /// </summary>
        [Pure]
        public static IObservable<float> Transpose(this IObservable<float> This,
                                                   float minFrom,
                                                   float maxFrom,
                                                   float minTo,
                                                   float maxTo) =>
            This.Select(
                x => x.Ratio(minFrom, maxFrom)
                      .Lerp(minTo, maxTo));

        /// <summary>
        /// Returns [minTo, maxTo] interpolation of given value within the [minFrom, maxFrom] interval,
        /// clipped to the [minTo, maxTo] interval
        /// </summary>
        [Pure]
        public static IObservable<float> TransposeClamp(this IObservable<float> This,
                                                        float minFrom,
                                                        float maxFrom,
                                                        float minTo,
                                                        float maxTo) =>
            This.Select(
                x => x.Ratio(minFrom, maxFrom)
                      .Clamp()
                      .Lerp(minTo, maxTo));

        #endregion

        #region Wrapping

        /// <summary>
        /// Returns wrapped value in order to fit within [from, to] range.
        /// </summary>
        [Pure]
        public static IObservable<float> Wrap(this IObservable<float> This, float min, float max) =>
            This.Select(x => x.Wrap(min, max));

        /// <summary>
        /// Returns wrapped value in order to fit within [0, max] range.
        /// </summary>
        [Pure]
        public static IObservable<float> Wrap(this IObservable<float> This, float max) =>
            This.Select(x => x.Wrap(max));

        #endregion

        #region Comparison

        [Pure]
        public static IObservable<bool> IsAlmostZero(this IObservable<float> This) =>
            This.Select(x => x.IsAlmostZero());

        [Pure]
        public static IObservable<bool> IsAlmostEqualTo(this IObservable<float> This, float other) =>
            This.Select(x => x.IsAlmostEqualTo(other));

        [Pure]
        public static IObservable<bool> IsAlmostEqualTo(this IObservable<float> This, float other, float epsilon) =>
            This.Select(x => x.IsAlmostEqualTo(other, epsilon));

        [Pure]
        public static IObservable<bool> IsWithin(this IObservable<float> This, float min, float max) =>
            This.Select(x => x.IsWithin(min, max));

        #endregion

        #region Rounding

        [Pure]
        public static IObservable<float> Round(this IObservable<float> This) =>
            This.Select(x => x.Round());

        [Pure]
        public static IObservable<int> RoundToInt(this IObservable<float> This) =>
            This.Select(x => x.RoundToInt());

        [Pure]
        public static IObservable<float> RoundToInterval(this IObservable<float> This, float interval) =>
            This.Select(x => x.RoundToInterval(interval));

        #endregion

        #region Easing

        /// <summary>
        /// Applies Hermite interpolation to smooth given ratio.
        /// </summary>
        [Pure]
        public static IObservable<float> EaseInEaseOut(this IObservable<float> This) =>
            This.Select(x => x.EaseInEaseOut());

        #endregion

        #region Clamping

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        [Pure]
        public static IObservable<float> Clamp(this IObservable<float> This, float min, float max) =>
            This.Select(x => x.Clamp(min, max));

        /// <summary>
        /// Returns value, either within the [min, max] interval or otherwise
        /// applying a certain percentage of elasticity to the excess.
        /// <param name="elasticity">0f is like normal clamping, 1f is like no clamping.</param>
        /// </summary>
        [Pure]
        public static IObservable<float> ElasticClamp(this IObservable<float> This,
                                                      float min,
                                                      float max,
                                                      float elasticity) =>
            This.Select(x => x.ElasticClamp(min, max, elasticity));

        /// <summary>
        /// Returns value clamped to the [0, 1] interval
        /// </summary>
        [Pure]
        public static IObservable<float> Clamp(this IObservable<float> This) =>
            This.Select(x => x.Clamp());

        /// <summary>
        /// Returns value clipped to the [min, +INF] interval
        /// </summary>
        [Pure]
        public static IObservable<float> Minimum(this IObservable<float> This, float min) =>
            This.Select(x => x.AtLeast(min));

        /// <summary>
        /// Returns value clipped to the [-INF, max] interval
        /// </summary>
        [Pure]
        public static IObservable<float> Maximum(this IObservable<float> This, float max) =>
            This.Select(x => x.AtMost(max));

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
        public static IObservable<float> Velocity(this IObservable<float> This,
                                                  float smoothness = Smoothness.Responsive,
                                                  IObservable<float> rawVelocities = null,
                                                  Func<float> getTime = null) =>
            new FloatVelocityObservable(This, smoothness, rawVelocities, getTime);

        #endregion
    }
}