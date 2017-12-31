using System;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class IObservableColorExtensions
    {
        #region Interpolation

        /// <summary>
        /// Uses This observable values as ratio to interpolate between source and target.
        /// </summary>
        [Pure]
        public static IObservable<Color> Lerp(this IObservable<float> This, Color source, Color target) =>
            This.Select(x => x.Lerp(source, target));

        #endregion
    }
}