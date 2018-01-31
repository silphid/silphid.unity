using System;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class DateTimeExtensions
    {
        #region Maths

        /// <summary>
        /// Returns absolute delta between two values.
        /// </summary>
        [Pure]
        public static DateTime Delta(this DateTime This, DateTime other) =>
            new DateTime(Math.Abs(This.Ticks - other.Ticks));

        [Pure]
        public static DateTime Average(this DateTime This, DateTime other) =>
            new DateTime((This.Ticks + other.Ticks) / 2);

        #endregion

        #region Ratio

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval
        /// </summary>
        [Pure]
        public static float Ratio(this DateTime value, DateTime min, DateTime max) =>
            value.Ticks.Ratio(min.Ticks, max.Ticks);

        #endregion

        #region Comparison

        /// <summary>
        /// Returns whether value lies within the [min, max] interval
        /// </summary>
        public static bool IsWithin(this DateTime value, DateTime min, DateTime max) =>
            value.Ticks.IsWithin(min.Ticks, max.Ticks);

        #endregion

        #region Wrapping

        /// <summary>
        /// Returns wrapped value in order to fit within [inclusiveMin, exclusiveMax[ range.
        /// </summary>
        public static DateTime Wrap(this DateTime value, DateTime inclusiveMin, DateTime exclusiveMax) =>
            new DateTime(value.Ticks.Wrap(inclusiveMin.Ticks, exclusiveMax.Ticks));

        /// <summary>
        /// Returns wrapped value in order to fit within [inclusiveMin, inclusiveMax] range.
        /// </summary>
        public static DateTime WrapInclusively(this DateTime value, DateTime inclusiveMin, DateTime inclusiveMax) =>
            new DateTime(value.Ticks.WrapInclusively(inclusiveMin.Ticks, inclusiveMax.Ticks));

        #endregion

        #region Clamping

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        [Pure]
        public static DateTime Clamp(this DateTime This, DateTime min, DateTime max) =>
            new DateTime(This.Ticks.Clamp(min.Ticks, max.Ticks));

        /// <summary>
        /// Returns the minimum value between this and another value
        /// </summary>
        public static DateTime Min(this DateTime This, long min) =>
            new DateTime(This.Ticks.Min(min));

        /// <summary>
        /// Returns the maximum value between this and another value
        /// </summary>
        public static DateTime Max(this DateTime This, long max) =>
            new DateTime(This.Ticks.Max(max));

        /// <summary>
        /// Returns value clamped to be greater than or at least equal to given value
        /// </summary>
        public static DateTime AtLeast(this DateTime This, long min) =>
            This.Max(min);

        /// <summary>
        /// Returns value clamped to be at less than or at most equal to given value
        /// </summary>
        public static DateTime AtMost(this DateTime This, long max) =>
            This.Min(max);

        /// <summary>
        /// Returns the minimum value between this and another value
        /// </summary>
        [Obsolete("Use AtLeast() instead")]
        public static DateTime Minimum(this DateTime This, long min) =>
            This.AtLeast(min);

        /// <summary>
        /// Returns the maximum value between this and another value
        /// </summary>
        [Obsolete("Use AtMost() instead")]
        public static DateTime Maximum(this DateTime This, long max) =>
            This.AtMost(max);

        #endregion
    }
}