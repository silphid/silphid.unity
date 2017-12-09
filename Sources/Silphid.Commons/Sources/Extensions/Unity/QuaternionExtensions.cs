using UnityEngine;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class QuaternionExtensions
    {
        #region Interpolation

        /// <summary>
        /// Uses This value as ratio to Quaternion.Lerp() between source and target.
        /// </summary>
        [Pure]
        public static Quaternion Lerp(this float This, Quaternion source, Quaternion target) =>
            Quaternion.Lerp(source, target, This);

        /// <summary>
        /// Uses This value as ratio to Quaternion.Slerp() between source and target.
        /// </summary>
        [Pure]
        public static Quaternion Slerp(this float This, Quaternion source, Quaternion target) =>
            Quaternion.Slerp(source, target, This);

        /// <summary>
        /// Uses This value as ratio and a single control handle to interpolate between source and target (quadratic Bézier).
        /// </summary>
        [Pure]
        public static Quaternion Bezier(this float This, Quaternion source, Quaternion target, Quaternion handle) =>
            This.Slerp(This.Slerp(source, handle), This.Slerp(handle, target));

        /// <summary>
        /// Uses This value as ratio and two control handles to interpolate between source and target (cubic Bézier).
        /// </summary>
        [Pure]
        public static Quaternion Bezier(this float This, Quaternion source, Quaternion target, Quaternion sourceHandle, Quaternion targetHandle)
        {
            var a = This.Slerp(source, sourceHandle);
            var b = This.Slerp(sourceHandle, targetHandle);
            var c = This.Slerp(targetHandle, target);
            return This.Slerp(This.Slerp(a, b), This.Slerp(b, c));
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
        public static Quaternion Smooth(this Quaternion This, Quaternion previousValue, float smoothness) =>
            smoothness.Lerp(This, previousValue);

        #endregion
    }
}