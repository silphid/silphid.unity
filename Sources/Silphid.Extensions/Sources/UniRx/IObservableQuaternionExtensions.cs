using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class IObservableQuaternionExtensions
    {
        #region Interpolation

        /// <summary>
        /// Uses This observable values as ratio to Quaternion.Lerp() between source and target.
        /// </summary>
        public static IObservable<Quaternion> Lerp(this IObservable<float> This, Quaternion source, Quaternion target) =>
            This.Select(x => x.Lerp(source, target));

        /// <summary>
        /// Uses This observable values as ratio to Quaternion.Slerp() between source and target.
        /// </summary>
        public static IObservable<Quaternion> Slerp(this IObservable<float> This, Quaternion source, Quaternion target) =>
            This.Select(x => x.Slerp(source, target));

        /// <summary>
        /// Uses This value as ratio and a single control handle to interpolate between source and target (quadratic Bézier).
        /// </summary>
        [Pure]
        public static IObservable<Quaternion> Bezier(this IObservable<float> This, Quaternion source, Quaternion target, Quaternion handle) =>
            This.Select(x => x.Bezier(source, target, handle));

        /// <summary>
        /// Uses This value as ratio and two control handles to interpolate between source and target (cubic Bézier).
        /// </summary>
        [Pure]
        public static IObservable<Quaternion> Bezier(this IObservable<float> This, Quaternion source, Quaternion target, Quaternion sourceHandle, Quaternion targetHandle) =>
            This.Select(x => x.Bezier(source, target, sourceHandle, targetHandle));


        #endregion
    }
}