using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class Int32Extensions
    {
        #region Ratio

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval
        /// </summary>
        [Pure]
        public static float Ratio(this int value, int min, int max) => (float)(value - min) / (max - min);

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval,
        /// clamped within that interval.
        /// </summary>
        [Pure]
        [Obsolete("Use value.Ratio(min, max).Clamp() instead")]
        public static float RatioClamp(this int value, int min, int max) =>
            value.Clamp(min, max).Ratio(min, max);

        #endregion

        /// <summary>
        /// Returns absolute delta between to values.
        /// </summary>
        public static int Distance(this int from, int to)
        {
            return Math.Abs(from - to);
        }

        /// <summary>
        /// Returns whether value lies within the [min, max] interval
        /// </summary>
        public static bool IsWithin(this int value, int min, int max)
        {
            return min < max
                ? value >= min && value <= max
                : value >= max && value <= min;
        }

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        public static int Clamp(this int value, int min, int inclusiveMax)
        {
            return min < inclusiveMax ? value.Minimum(min).Maximum(inclusiveMax) : value.Minimum(inclusiveMax).Maximum(min);
        }

        /// <summary>
        /// Returns value clamped to the [min, max[ interval
        /// </summary>
        public static int ClampExclusively(this int value, int min, int exclusiveMax)
        {
            return value.Clamp(min, exclusiveMax - 1);
        }

        /// <summary>
        /// Returns value clipped to the [min, +INF] interval
        /// </summary>
        public static int Minimum(this int value, int min)
        {
            return Math.Max(value, min);
        }

        /// <summary>
        /// Returns value clipped to the [-INF, max] interval
        /// </summary>
        public static int Maximum(this int value, int max)
        {
            return Math.Min(value, max);
        }

        /// <summary>
        /// Returns absolute value of this value
        /// </summary>
        public static int Abs(this int value)
        {
            return Math.Abs(value);
        }

        /// <summary>
        /// Returns wrapped value in order to fit within [inclusiveMin, exclusiveMax[ range.
        /// </summary>
        public static int Wrap(this int value, int inclusiveMin, int exclusiveMax)
        {
            return inclusiveMin + Wrap(value - inclusiveMin, exclusiveMax - inclusiveMin);
        }

        /// <summary>
        /// Returns wrapped value in order to fit within [0, exclusiveMax[ range.
        /// </summary>
        public static int Wrap(this int value, int exclusiveMax)
        {
            return value >= 0
                ? value % exclusiveMax
                : (exclusiveMax + value % exclusiveMax) % exclusiveMax;
        }

        /// <summary>
        /// Returns wrapped value in order to fit within [inclusiveMin, inclusiveMax] range.
        /// </summary>
        public static int WrapInclusively(this int value, int inclusiveMin, int inclusiveMax)
        {
            return value.Wrap(inclusiveMin, inclusiveMax + 1);
        }

        /// <summary>
        /// Returns wrapped value in order to fit within [0, inclusiveMax] range.
        /// </summary>
        public static int WrapInclusively(this int value, int inclusiveMax)
        {
            return value.Wrap(inclusiveMax + 1);
        }

        /// <summary>
        /// Returns a string with the invariant culture.
        /// </summary>
        public static string ToInvariantString(this int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}