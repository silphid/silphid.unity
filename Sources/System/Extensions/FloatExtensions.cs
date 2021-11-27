using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class FloatExtensions
    {
        #region Maths

        [Pure]
        public static int Sign(this float This) => Math.Sign(This);

        [Pure]
        public static float Abs(this float This) => Math.Abs(This);

        [Pure]
        public static float Negate(this float This) => -This;

        [Pure]
        public static float Floor(this float This) => (float) Math.Floor(This);

        [Pure]
        public static float Ceiling(this float This) => (float) Math.Ceiling(This);

        [Pure]
        public static int FloorInt(this float This) => (int) Math.Floor(This);

        [Pure]
        public static int CeilingInt(this float This) => (int) Math.Ceiling(This);

        /// <summary>
        /// Returns fractional part of given value.
        /// </summary>
        [Pure]
        public static float Fractional(this float This) => This - This.Floor();

        /// <summary>
        /// Returns absolute delta between two values.
        /// </summary>
        [Pure]
        public static float Delta(this float This, float other) => Math.Abs(This - other);

        [Pure]
        public static float Average(this float This, float other) => (This + other) / 2;

        #endregion

        #region Interpolation

        /// <summary>
        /// Uses This as ratio to linearly interpolate between source and target.
        /// </summary>
        [Pure]
        public static float Lerp(this float This, float source, float target) =>
            source + (target - source) * This;

        /// <summary>
        /// Uses This as ratio to linearly interpolate between source and target.
        /// </summary>
        [Pure]
        public static int Lerp(this float This, int source, int target) =>
            (int) (source + (target - source) * This);

        /// <summary>
        /// Uses This as ratio to linearly interpolate between source and target.
        /// </summary>
        [Pure]
        public static long Lerp(this float This, long source, long target) =>
            (long) (source + (target - source) * This);

        /// <summary>
        /// Uses This as ratio to linearly interpolate between source and target.
        /// </summary>
        [Pure]
        public static DateTime Lerp(this float This, DateTime source, DateTime target) =>
            new DateTime(This.Lerp(source.Ticks, target.Ticks));

        /// <summary>
        /// Uses This as ratio to linearly interpolate between source and target.
        /// </summary>
        [Pure]
        public static TimeSpan Lerp(this float This, TimeSpan source, TimeSpan target) =>
            new TimeSpan(This.Lerp(source.Ticks, target.Ticks));

        /// <summary>
        /// Uses This value as ratio and a single control handle to interpolate between source and target (quadratic Bézier).
        /// </summary>
        [Pure]
        public static float Bezier(this float This, float source, float target, float handle) =>
            This.Lerp(This.Lerp(source, handle), This.Lerp(handle, target));

        /// <summary>
        /// Uses This value as ratio and two control handles to interpolate between source and target (cubic Bézier).
        /// </summary>
        [Pure]
        public static float Bezier(this float This, float source, float target, float sourceHandle, float targetHandle)
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
        public static float Smooth(this float This, float previousValue, float smoothness = Smoothness.Default) =>
            smoothness.Lerp(This, previousValue);

        #endregion

        #region Ratio

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval
        /// </summary>
        [Pure]
        public static float Ratio(this float value, float min, float max) => (value - min) / (max - min);

        #endregion

        #region Transposition

        /// <summary>
        /// Returns [minTo, maxTo] interpolation of given value within the [minFrom, maxFrom] interval
        /// </summary>
        [Pure]
        public static float Transpose(this float This, float minFrom, float maxFrom, float minTo, float maxTo) =>
            This.Ratio(minFrom, maxFrom)
                .Lerp(minTo, maxTo);

        /// <summary>
        /// Returns [minTo, maxTo] interpolation of given value within the [minFrom, maxFrom] interval,
        /// clipped to the [minTo, maxTo] interval
        /// </summary>
        [Pure]
        public static float TransposeClamp(this float This, float minFrom, float maxFrom, float minTo, float maxTo) =>
            This.Transpose(minFrom, maxFrom, minTo, maxTo)
                .Clamp(minTo, maxTo);

        #endregion

        #region Wrapping

        /// <summary>
        /// Returns wrapped value in order to fit within [min, max] range.
        /// </summary>
        [Pure]
        public static float Wrap(this float This, float min, float max) =>
            Ratio(This, min, max)
               .Fractional()
               .Lerp(min, max);

        /// <summary>
        /// Returns wrapped value in order to fit within [0, max] range.
        /// </summary>
        [Pure]
        public static float Wrap(this float This, float max) => (This / max).Fractional() * max;

        #endregion

        #region Comparison

        private const float Epsilon = 0.0001f;

        [Pure]
        public static bool IsAlmostZero(this float This) => This.IsAlmostEqualTo(0f);

        [Pure]
        public static bool IsAlmostEqualTo(this float This, float other) =>
            IsAlmostEqualTo(This, other, Epsilon);

        [Pure]
        public static bool IsAlmostEqualTo(this float This, float other, float epsilon) =>
            Math.Abs(This - other) <= epsilon;

        [Pure]
        public static bool IsWithin(this float This, float min, float max) =>
            min < max
                ? min < This && This < max
                : max < This && This < min;

        #endregion

        #region Rounding

        [Pure]
        public static float Round(this float This) => (float) Math.Round(This);

        [Pure]
        public static int RoundToInt(this float This) => (int) Math.Round(This);

        [Pure]
        public static float RoundToInterval(this float This, float interval) =>
            (This / interval).Round() * interval;

        #endregion

        #region Easing

        /// <summary>
        /// Applies Hermite interpolation to smooth given ratio
        /// </summary>
        [Pure]
        public static float EaseInEaseOut(this float This) => This * This * (3 - 2 * This);

        #endregion

        #region Clamping

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        [Pure]
        public static float Clamp(this float This, float min, float max) =>
            min < max
                ? This.AtLeast(min)
                      .AtMost(max)
                : This.AtLeast(max)
                      .AtMost(min);

        /// <summary>
        /// Returns value clamped to the [0, 1] interval
        /// </summary>
        [Pure]
        public static float Clamp(this float This) => This.Clamp(0, 1);

        /// <summary>
        /// Returns value, either within the [min, max] interval or otherwise
        /// applying a certain percentage of elasticity to the excess.
        /// <param name="elasticity">0f is like normal clamping, 1f is like no clamping.</param>
        /// </summary>
        [Pure]
        public static float ElasticClamp(this float This, float min, float max, float elasticity)
        {
            if (min > max)
            {
                var tmp = min;
                min = max;
                max = tmp;
            }

            if (This > max)
                return max + (This - max) * elasticity;

            if (This < min)
                return min - (min - This) * elasticity;

            return This;
        }

        /// <summary>
        /// Returns the minimum value between this and another value
        /// </summary>
        public static float Min(this float This, float min) =>
            Math.Min(This, min);

        /// <summary>
        /// Returns the maximum value between this and another value
        /// </summary>
        public static float Max(this float This, float max) =>
            Math.Max(This, max);

        /// <summary>
        /// Returns value clamped to be greater than or at least equal to given value
        /// </summary>
        public static float AtLeast(this float This, float min) =>
            This.Max(min);

        /// <summary>
        /// Returns value clamped to be at less than or at most equal to given value
        /// </summary>
        public static float AtMost(this float This, float max) =>
            This.Min(max);

        /// <summary>
        /// Returns the minimum value between this and another value
        /// </summary>
        [Obsolete("Use AtLeast() instead")]
        public static float Minimum(this float This, float min) =>
            This.AtLeast(min);

        /// <summary>
        /// Returns the maximum value between this and another value
        /// </summary>
        [Obsolete("Use AtMost() instead")]
        public static float Maximum(this float This, float max) =>
            This.AtMost(max);

        #endregion

        #region Formatting

        [Pure]
        public static string ToInvariantString(this float This) =>
            This.ToString(CultureInfo.InvariantCulture);

        #endregion
    }
}