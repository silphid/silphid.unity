using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class DoubleExtensions
    {
        #region Maths

        [Pure]
        public static int Sign(this double This) => Math.Sign(This);

        [Pure]
        public static double Abs(this double This) => Math.Abs(This);

        [Pure]
        public static double Negate(this double This) => -This;

        [Pure]
        public static double Floor(this double This) => Math.Floor(This);

        [Pure]
        public static double Ceiling(this double This) => Math.Ceiling(This);

        [Pure]
        public static int FloorToInt(this double This) => (int) Math.Floor(This);

        [Pure]
        public static int CeilingToInt(this double This) => (int) Math.Ceiling(This);

        /// <summary>
        /// Returns fractional part of given value.
        /// </summary>
        [Pure]
        public static double Fractional(this double This) => This - Floor(This);

        /// <summary>
        /// Returns absolute delta between two values.
        /// </summary>
        [Pure]
        public static double Delta(this double This, double other) => Math.Abs(This - other);

        [Pure]
        public static double Average(this double This, double other) => (This + other) / 2;

        #endregion

        #region Interpolation

        /// <summary>
        /// Uses This as ratio to linearly interpolate between source and target.
        /// </summary>
        [Pure]
        public static double Lerp(this double This, double source, double target) =>
            source + (target - source) * This;

        /// <summary>
        /// Uses This as ratio to linearly interpolate between source and target.
        /// </summary>
        [Pure]
        public static int Lerp(this double This, int source, int target) =>
            (int)(source + (target - source) * This);

        /// <summary>
        /// Uses This as ratio to linearly interpolate between source and target.
        /// </summary>
        [Pure]
        public static long Lerp(this double This, long source, long target) =>
            (long)(source + (target - source) * This);

        /// <summary>
        /// Uses This as ratio to linearly interpolate between source and target.
        /// </summary>
        [Pure]
        public static DateTime Lerp(this double This, DateTime source, DateTime target) =>
            new DateTime(Lerp(This, source.Ticks, target.Ticks));

        /// <summary>
        /// Uses This value as ratio and a single control handle to interpolate between source and target (quadratic Bézier).
        /// </summary>
        [Pure]
        public static double Bezier(this double This, double source, double target, double handle) =>
            Lerp(This, Lerp(This, source, handle), Lerp(This, handle, target));

        /// <summary>
        /// Uses This value as ratio and two control handles to interpolate between source and target (cubic Bézier).
        /// </summary>
        [Pure]
        public static double Bezier(this double This, double source, double target, double sourceHandle, double targetHandle)
        {
            var a = Lerp(This, source, sourceHandle);
            var b = Lerp(This, sourceHandle, targetHandle);
            var c = Lerp(This, targetHandle, target);
            return Lerp(This, Lerp(This, a, b), Lerp(This, b, c));
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
        public static double Smooth(this double This, double previousValue, double smoothness) =>
            Lerp(smoothness, This, previousValue);

        #endregion

        #region Ratio

        /// <summary>
        /// Returns [0, 1] ratio of given value within the [min, max] interval
        /// </summary>
        [Pure]
        public static double Ratio(this double value, double min, double max) => (value - min) / (max - min);

        #endregion

        #region Transposition

        /// <summary>
        /// Returns [minTo, maxTo] interpolation of given value within the [minFrom, maxFrom] interval
        /// </summary>
        [Pure]
        public static double Transpose(this double This, double minFrom, double maxFrom, double minTo, double maxTo) =>
            Lerp(Ratio(This, minFrom, maxFrom), minTo, maxTo);

        /// <summary>
        /// Returns [minTo, maxTo] interpolation of given value within the [minFrom, maxFrom] interval,
        /// clipped to the [minTo, maxTo] interval
        /// </summary>
        [Pure]
        public static double TransposeClamp(this double This, double minFrom, double maxFrom, double minTo, double maxTo) =>
            Clamp(Transpose(This, minFrom, maxFrom, minTo, maxTo), minTo, maxTo);

        #endregion

        #region Wrapping

        /// <summary>
        /// Returns wrapped value in order to fit within [min, max] range.
        /// </summary>
        [Pure]
        public static double Wrap(this double This, double min, double max) =>
            Lerp(Fractional(Ratio(This, min, max)), min, max);

        /// <summary>
        /// Returns wrapped value in order to fit within [0, max] range.
        /// </summary>
        [Pure]
        public static double Wrap(this double This, double max) => Fractional((This / max)) * max;

        #endregion

        #region Comparison

        private const double Epsilon = 0.0001f;

        [Pure]
        public static bool IsAlmostZero(this double This) => IsAlmostEqualTo(This, 0f);

        [Pure]
        public static bool IsAlmostEqualTo(this double This, double other) =>
            IsAlmostEqualTo(This, other, Epsilon);

        [Pure]
        public static bool IsAlmostEqualTo(this double This, double other, double epsilon) =>
            Math.Abs(This - other) <= epsilon;

        [Pure]
        public static bool IsWithin(this double This, double min, double max) =>
            min < max ? min < This && This < max : max < This && This < min;

        #endregion

        #region Rounding

        [Pure]
        public static double Round(this double This) => Math.Round(This);

        [Pure]
        public static int RoundToInt(this double This) => (int) Math.Round(This);

        [Pure]
        public static double RoundToInterval(this double This, double interval) =>
            Round(This / interval) * interval;

        #endregion

        #region Easing

        /// <summary>
        /// Applies Hermite interpolation to smooth given ratio
        /// </summary>
        [Pure]
        public static double EaseInEaseOut(this double This) => This * This * (3 - 2 * This);

        #endregion

        #region Clamping

        /// <summary>
        /// Returns value clamped to the [min, max] interval
        /// </summary>
        [Pure]
        public static double Clamp(this double This, double min, double max) =>
            min < max ? AtMost(AtLeast(This, min), max) : AtMost(AtLeast(This, max), min);

        /// <summary>
        /// Returns value clamped to the [0, 1] interval
        /// </summary>
        [Pure]
        public static double Clamp(this double This) => Clamp(This, 0, 1);

        /// <summary>
        /// Returns value clipped to the [min, +INF] interval
        /// </summary>
        [Pure]
        public static double AtLeast(this double value, double min) => Math.Max(value, min);

        /// <summary>
        /// Returns value clipped to the [-INF, max] interval
        /// </summary>
        [Pure]
        public static double AtMost(this double value, double max) => Math.Min(value, max);

        [Pure]
        public static double Min(this double value, double min) => Math.Min(value, min);

        [Pure]
        public static double Max(this double value, double max) => Math.Max(value, max);

        #endregion

        #region Formatting

        [Pure]
        public static string ToInvariantString(this double This) =>
            This.ToString(CultureInfo.InvariantCulture);

        #endregion
    }
}