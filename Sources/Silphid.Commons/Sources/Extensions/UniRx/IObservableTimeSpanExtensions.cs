using System;
using JetBrains.Annotations;
using UniRx;

namespace Silphid.Extensions
{
    public static class IObservableTimeSpanExtensions
    {
        #region Interpolation

        /// <summary>
        /// Uses This observable values as ratio to interpolate between source and target.
        /// </summary>
        [Pure]
        public static IObservable<TimeSpan> Lerp(this IObservable<float> This, TimeSpan source, TimeSpan target) =>
            This.Select(x => x.Lerp(source, target));

        #endregion
    }
}