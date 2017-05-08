using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class LongExtensions
    {
        #region Ratio

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval
        /// </summary>
        [Pure]
        public static float Ratio(this long value, long min, long max) => (float)(value - min) / (max - min);

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval,
        /// clamped within that interval.
        /// </summary>
        [Pure]
        [Obsolete("Use value.Ratio(min, max).Clamp() instead")]
        public static float RatioClamp(this long value, long min, long max) =>
            value.Clamp(min, max).Ratio(min, max);

        #endregion

        /// <summary>
        /// Returns absolute delta between to values.
        /// </summary>
        public static long Distance(this long from, long to)
        {
            return Math.Abs(from - to);
        }

        /// <summary>
        /// Returns whether value lies within the [min, max] interval
        /// </summary>
        public static bool IsWithin(this long value, long min, long max)
        {
            return min < max
                ? value >= min && value <= max
                : value >= max && value <= min;
        }

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        public static long Clamp(this long value, long min, long inclusiveMax)
        {
            return min < inclusiveMax ? value.Minimum(min).Maximum(inclusiveMax) : value.Minimum(inclusiveMax).Maximum(min);
        }

        /// <summary>
        /// Returns value clamped to the [min, max[ interval
        /// </summary>
        public static long ClampExclusively(this long value, long min, long exclusiveMax)
        {
            return value.Clamp(min, exclusiveMax - 1);
        }

        /// <summary>
        /// Returns value clipped to the [min, +INF] interval
        /// </summary>
        public static long Minimum(this long value, long min)
        {
            return Math.Max(value, min);
        }

        /// <summary>
        /// Returns value clipped to the [-INF, max] interval
        /// </summary>
        public static long Maximum(this long value, long max)
        {
            return Math.Min(value, max);
        }

        /// <summary>
        /// Returns absolute value of this value
        /// </summary>
        public static long Abs(this long value)
        {
            return Math.Abs(value);
        }

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

        /// <summary>
        /// Returns a string with the invariant culture.
        /// </summary>
        public static string ToInvariantString(this long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}