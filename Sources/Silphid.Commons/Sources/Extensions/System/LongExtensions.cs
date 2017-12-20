using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class LongExtensions
    {
        #region Maths

        [Pure]
        public static long Sign(this long This) =>
            Math.Sign(This);

        /// <summary>
        /// Returns absolute value of this value
        /// </summary>
        [Pure]
        public static long Abs(this long value) =>
            Math.Abs(value);

        [Pure]
        public static long Negate(this long This) =>
            -This;

        /// <summary>
        /// Returns absolute delta between two values.
        /// </summary>
        [Pure]
        public static long Delta(this long This, long to) =>
            Math.Abs(This - to);

        [Pure]
        public static long Average(this long This, long other) =>
            (This + other) / 2;

        #endregion

        #region Ratio

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval
        /// </summary>
        [Pure]
        public static float Ratio(this long value, long min, long max) =>
            (float)(value - min) / (max - min);

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval,
        /// clamped within that interval.
        /// </summary>
        [Pure]
        [Obsolete("Use value.Ratio(min, max).Clamp() instead")]
        public static float RatioClamp(this long value, long min, long max) =>
            value.Clamp(min, max).Ratio(min, max);

        #endregion

        #region Comparison

        /// <summary>
        /// Returns whether value lies within the [min, max] interval
        /// </summary>
        public static bool IsWithin(this long This, long min, long max) =>
            min < max
                ? This >= min && This <= max
                : This >= max && This <= min;

        #endregion

        #region Clamping

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        public static long Clamp(this long This, long min, long inclusiveMax) =>
            min < inclusiveMax
                ? This.AtLeast(min).AtMost(inclusiveMax)
                : This.AtLeast(inclusiveMax).AtMost(min);

        /// <summary>
        /// Returns value clamped to the [min, max[ interval
        /// </summary>
        public static long ClampExclusively(this long This, long min, long exclusiveMax) =>
            This.Clamp(min, exclusiveMax - 1);

        /// <summary>
        /// Returns the minimum value between this and another value
        /// </summary>
        public static long Min(this long This, long min) =>
            Math.Min(This, min);

        /// <summary>
        /// Returns the maximum value between this and another value
        /// </summary>
        public static long Max(this long This, long max) =>
            Math.Max(This, max);

        /// <summary>
        /// Returns value clamped to be greater than or at least equal to given value
        /// </summary>
        public static long AtLeast(this long This, long min) =>
            This.Max(min);

        /// <summary>
        /// Returns value clamped to be at less than or at most equal to given value
        /// </summary>
        public static long AtMost(this long This, long max) =>
            This.Min(max);

        #endregion

        #region Wrapping

        /// <summary>
        /// Returns wrapped value in order to fit within [inclusiveMin, exclusiveMax[ range.
        /// </summary>
        public static long Wrap(this long value, long inclusiveMin, long exclusiveMax)
        {
            return inclusiveMin + Wrap(value - inclusiveMin, exclusiveMax - inclusiveMin);
        }

        /// <summary>
        /// Returns wrapped value in order to fit within [0, exclusiveMax[ range.
        /// </summary>
        public static long Wrap(this long value, long exclusiveMax)
        {
            return value >= 0
                ? value % exclusiveMax
                : (exclusiveMax + value % exclusiveMax) % exclusiveMax;
        }

        /// <summary>
        /// Returns wrapped value in order to fit within [inclusiveMin, inclusiveMax] range.
        /// </summary>
        public static long WrapInclusively(this long value, long inclusiveMin, long inclusiveMax)
        {
            return value.Wrap(inclusiveMin, inclusiveMax + 1);
        }

        /// <summary>
        /// Returns wrapped value in order to fit within [0, inclusiveMax] range.
        /// </summary>
        public static long WrapInclusively(this long value, long inclusiveMax)
        {
            return value.Wrap(inclusiveMax + 1);
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Returns a string with the invariant culture.
        /// </summary>
        public static string ToInvariantString(this long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToZeroPaddedString(this long value, long digits)
        {
            var format = "{0:D" + digits + "}";
            return string.Format(format, value);
        }

        #endregion
    }
}