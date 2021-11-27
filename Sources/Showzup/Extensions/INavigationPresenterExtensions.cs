using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Showzup
{
    public static class INavigationPresenterExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(INavigationPresenter));

        public static IObservable<IView> TryPop(this INavigationPresenter This) =>
            This.CanPop.Value
                ? This.Pop()
                : Observable.Empty<IView>();

        public static IObservable<IView> TryPopTo(this INavigationPresenter This, Nav nav) =>
            This.CanPop.Value
                ? This.PopTo(nav)
                : Observable.Empty<IView>();

        public static IObservable<IView> TryPopTo(this INavigationPresenter This, IView view) =>
            This.CanPop.Value
                ? This.PopTo(view)
                : Observable.Empty<IView>();

        public static IObservable<IView> TryPopToRoot(this INavigationPresenter This) =>
            This.CanPop.Value
                ? This.PopToRoot()
                : Observable.Empty<IView>();

        public static IObservable<IView> PopTo(this INavigationPresenter This, IView view) =>
            Observable.Defer(
                () =>
                {
                    var nav = This.History.Value.FirstOrDefault(x => x.View == view);
                    return This.PopTo(nav);
                });


        public static void DropFromHistory(this INavigationPresenter This, int count)
        {
            Log.Debug($"DropFromHistory({count}) Requested");

            This.Ready()
                .FirstTrue()
                .Subscribe(
                     _ =>
                     {
                         if (!This.IsReady())
                         {
                             Log.Warn("Attempting to change history while not ready");
                             return;
                         }

                         if (count > This.History.Value.Count)
                         {
                             Log.Warn(
                                 $"Cannot remove {count} views from history because it only contains {This.History.Value.Count}");
                             return;
                         }

                         Log.Debug($"DropFromHistory({count})");
                         This.History.Value = This.History.Value.Take(This.History.Value.Count - count)
                                                  .ToList();
                     });
        }

        /// <summary>
        /// Invalidate navigation history
        /// </summary>
        /// <param name="This"></param>
        /// <param name="includeCurrent">Invalidate current page</param>
        public static void InvalidateHistory(this INavigationPresenter This, bool includeCurrent = false)
        {
            Log.Debug($"Invalidating history including current: {includeCurrent}");

            var count = includeCurrent
                            ? This.History.Value.Count
                            : This.History.Value.Count - 1;

            This.History.Value.Take(count)
                .ForEach(x => x.Invalidate());
        }

        public static void DropAllHistory(this INavigationPresenter This)
        {
            Log.Debug("DropAllHistory Requested");

            This.Ready()
                .FirstTrue()
                .Subscribe(
                     _ =>
                     {
                         if (!This.IsReady())
                         {
                             Log.Warn("Attempting to change history while not ready");
                             return;
                         }

                         Log.Debug("DropAllHistory");
                         This.History.Value = This.History.Value.LastOrDefault()
                                                 ?.ToSingleItemList() ?? new List<Nav>();
                     });
        }
    }
}