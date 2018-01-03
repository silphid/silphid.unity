using System;
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
        
        public static IObservable<IView> TryPopTo(this INavigationPresenter This, IView view) =>
            This.CanPop.Value
                ? This.PopTo(view)
                : Observable.Empty<IView>();

        public static void DropFromHistory(this INavigationPresenter This, int count)
        {
            Log.Debug($"DropFromHistory({count})");
            This.AssertCanAlterHistory();

            if (count > This.History.Value.Count)
                throw new InvalidOperationException($"Cannot remove {count} views from history because it only contains {This.History.Value.Count}");

            This.History.Value = This.History.Value
                .Take(This.History.Value.Count - count)
                .ToList();
        }

        public static void ClearHistory(this INavigationPresenter This)
        {
            Log.Debug("ClearHistory()");
            This.AssertCanAlterHistory();

            This.History.Value = This.History.Value.Last().ToSingleItemList();
        }

        private static void AssertCanAlterHistory(this INavigationPresenter This)
        {
            if (!This.IsReady())
                throw new InvalidOperationException("Cannot alter history during navigation");
        }
    }
}