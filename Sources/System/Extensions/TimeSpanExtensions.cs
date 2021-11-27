using System;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class TimeSpanExtensions
    {
        #region Maths

        /// <summary>
        /// Returns absolute delta between two values.
        /// </summary>
        [Pure]
        public static TimeSpan Delta(this TimeSpan This, TimeSpan other) =>
            new TimeSpan(Math.Abs(This.Ticks - other.Ticks));

        [Pure]
        public static TimeSpan Average(this TimeSpan This, TimeSpan other) =>
            new TimeSpan((This.Ticks + other.Ticks) / 2);

        [Pure]
        public static TimeSpan Multiply(this TimeSpan timeSpan, double factor)
        {
            return TimeSpan.FromSeconds(timeSpan.TotalSeconds * factor);
        }

        [Pure]
        public static TimeSpan TotalDaysFractional(this TimeSpan timeSpan)
        {
            return TimeSpan.FromDays(((float) timeSpan.TotalDays).Fractional());
        }

        #endregion

        #region Ratio

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval
        /// </summary>
        [Pure]
        public static float Ratio(this TimeSpan value, TimeSpan min, TimeSpan max) =>
            value.Ticks.Ratio(min.Ticks, max.Ticks);

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [0, max] interval
        /// </summary>
        [Pure]
        public static float Ratio(this TimeSpan value, TimeSpan max) =>
            value.Ticks.Ratio(0, max.Ticks);

        #endregion

        #region Comparison

        /// <summary>
        /// Returns whether value lies within the [min, max] interval
        /// </summary>
        public static bool IsWithin(this TimeSpan value, TimeSpan min, TimeSpan max) =>
            value.Ticks.IsWithin(min.Ticks, max.Ticks);

        #endregion

        #region Wrapping

        /// <summary>
        /// Returns wrapped value in order to fit within [inclusiveMin, exclusiveMax[ range.
        /// </summary>
        public static TimeSpan Wrap(this TimeSpan value, TimeSpan inclusiveMin, TimeSpan exclusiveMax) =>
            new TimeSpan(value.Ticks.Wrap(inclusiveMin.Ticks, exclusiveMax.Ticks));

        /// <summary>
        /// Returns wrapped value in order to fit within [inclusiveMin, inclusiveMax] range.
        /// </summary>
        public static TimeSpan WrapInclusively(this TimeSpan value, TimeSpan inclusiveMin, TimeSpan inclusiveMax) =>
            new TimeSpan(value.Ticks.WrapInclusively(inclusiveMin.Ticks, inclusiveMax.Ticks));

        #endregion

        #region Clamping

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        [Pure]
        public static TimeSpan Clamp(this TimeSpan This, TimeSpan min, TimeSpan max) =>
            new TimeSpan(This.Ticks.Clamp(min.Ticks, max.Ticks));

        /// <summary>
        /// Returns the minimum value between this and another value
        /// </summary>
        public static TimeSpan Min(this TimeSpan This, long min) =>
            TimeSpan.FromTicks(This.Ticks.Min(min));

        /// <summary>
        /// Returns the maximum value between this and another value
        /// </summary>
        public static TimeSpan Max(this TimeSpan This, long max) =>
            TimeSpan.FromTicks(This.Ticks.Max(max));

        /// <summary>
        /// Returns value clamped to be greater than or at least equal to given value
        /// </summary>
        public static TimeSpan AtLeast(this TimeSpan This, long min) =>
            This.Max(min);

        /// <summary>
        /// Returns value clamped to be at less than or at most equal to given value
        /// </summary>
        public static TimeSpan AtMost(this TimeSpan This, long max) =>
            This.Min(max);

        /// <summary>
        /// Returns the minimum value between this and another value
        /// </summary>
        [Obsolete("Use AtLeast() instead")]
        public static TimeSpan Minimum(this TimeSpan This, long min) =>
            This.AtLeast(min);

        /// <summary>
        /// Returns the maximum value between this and another value
        /// </summary>
        [Obsolete("Use AtMost() instead")]
        public static TimeSpan Maximum(this TimeSpan This, long max) =>
            This.AtMost(max);

        #endregion
    }
}