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
        /// Returns value clipped to the [min, +INF] interval
        /// </summary>
        [Pure]
        public static DateTime Minimum(this DateTime value, DateTime min) =>
            new DateTime(value.Ticks.Minimum(min.Ticks));

        /// <summary>
        /// Returns value clipped to the [-INF, max] interval
        /// </summary>
        [Pure]
        public static DateTime Maximum(this DateTime value, DateTime max) =>
            new DateTime(value.Ticks.Maximum(max.Ticks));

        #endregion
    }
}