using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class Int32Extensions
    {
        #region Maths

        [Pure]
        public static int Sign(this int This) => Math.Sign(This);

        /// <summary>
        /// Returns absolute value of this value
        /// </summary>
        [Pure]
        public static int Abs(this int value) => Math.Abs(value);

        [Pure]
        public static int Negate(this int This) => -This;

        /// <summary>
        /// Returns absolute delta between two values.
        /// </summary>
        [Pure]
        public static int Delta(this int This, int to) => Math.Abs(This - to);

        [Pure]
        public static int Average(this int This, int other) => (This + other) / 2;

        #endregion

        #region Ratio

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval
        /// </summary>
        [Pure]
        public static float Ratio(this int value, int min, int max) => (float) (value - min) / (max - min);

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval,
        /// clamped within that interval.
        /// </summary>
        [Pure]
        [Obsolete("Use value.Ratio(min, max).Clamp() instead")]
        public static float RatioClamp(this int value, int min, int max) =>
            value.Clamp(min, max)
                 .Ratio(min, max);

        #endregion

        #region Comparison

        /// <summary>
        /// Returns whether value lies within the [min, max] interval
        /// </summary>
        public static bool IsWithin(this int This, int min, int max) =>
            min < max
                ? This >= min && This <= max
                : This >= max && This <= min;

        #endregion

        #region Clamping

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        public static int Clamp(this int value, int min, int inclusiveMax)
        {
            return min < inclusiveMax
                       ? value.AtLeast(min)
                              .AtMost(inclusiveMax)
                       : value.AtLeast(inclusiveMax)
                              .AtMost(min);
        }

        /// <summary>
        /// Returns value clamped to the [min, max[ interval
        /// </summary>
        public static int ClampExclusively(this int value, int min, int exclusiveMax)
        {
            return value.Clamp(min, exclusiveMax - 1);
        }

        /// <summary>
        /// Returns the minimum value between this and another value
        /// </summary>
        public static int Min(this int This, int min) =>
            Math.Min(This, min);

        /// <summary>
        /// Returns the maximum value between this and another value
        /// </summary>
        public static int Max(this int This, int max) =>
            Math.Max(This, max);

        /// <summary>
        /// Returns value clamped to be greater than or at least equal to given value
        /// </summary>
        public static int AtLeast(this int This, int min) =>
            This.Max(min);

        /// <summary>
        /// Returns value clamped to be at less than or at most equal to given value
        /// </summary>
        public static int AtMost(this int This, int max) =>
            This.Min(max);

        /// <summary>
        /// Returns the minimum value between this and another value
        /// </summary>
        [Obsolete("Use AtLeast() instead")]
        public static int Minimum(this int This, int min) =>
            This.AtLeast(min);

        /// <summary>
        /// Returns the maximum value between this and another value
        /// </summary>
        [Obsolete("Use AtMost() instead")]
        public static int Maximum(this int This, int max) =>
            This.AtMost(max);

        /// <summary>
        /// Returns the nearest multiple of given factor that is lower than or equal to this value.
        /// </summary>
        public static int FloorMultipleOf(this int This, int factor) =>
            This - This % factor;

        /// <summary>
        /// Returns the nearest multiple of given factor that is greater than or equal to this value.
        /// </summary>
        public static int CeilingMultipleOf(this int This, int factor) =>
            (This + factor - 1) * factor / factor;

        /// <summary>
        /// Returns the division of This number by given divider, rounded up to nearest higher integer.
        /// 
        /// Ex: 0.CeilingDivisionBy(4) == 0
        ///     1.CeilingDivisionBy(4) == 1
        ///     2.CeilingDivisionBy(4) == 1
        ///     3.CeilingDivisionBy(4) == 1
        ///     4.CeilingDivisionBy(4) == 1
        ///     5.CeilingDivisionBy(4) == 2
        ///     6.CeilingDivisionBy(4) == 2
        /// </summary>
        public static int CeilingDivisionBy(this int This, int divider) =>
            (This + divider - 1) / divider;

        #endregion

        #region Wrapping

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

        #endregion

        #region Formatting

        /// <summary>
        /// Returns a string with the invariant culture.
        /// </summary>
        public static string ToInvariantString(this int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToZeroPaddedString(this int value, int digits)
        {
            var format = "{0:D" + digits + "}";
            return string.Format(format, value);
        }

        #endregion
    }
}