using System;
using JetBrains.Annotations;
using UniRx;

namespace Silphid.Extensions
{
    public static class IObservableDateTimeExtensions
    {
        #region Interpolation

        /// <summary>
        /// Uses This observable values as ratio to interpolate between source and target.
        /// </summary>
        [Pure]
        public static IObservable<DateTime> Lerp(this IObservable<float> This, DateTime source, DateTime target) =>
            This.Select(x => x.Lerp(source, target));

        #endregion
    }
}