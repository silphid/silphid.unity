using System;
using Silphid.Extensions;
using UniRx;
using Rx = UniRx;

namespace Silphid.Showzup
{
    public static class IObservableNavExtensions
    {
        #region Rx.IObservable<object>

        public static IDisposable BindTo(this Rx.IObservable<object> This, IPresenter target) =>
            This.Subscribe(x => target.Present(x).SubscribeAndForget());

        #endregion

        #region Rx.IObservable<Nav>

        public static Rx.IObservable<ViewNav<TView>> From<TView>(this Rx.IObservable<Nav> This) where TView : IView =>
            This.Where(x => x.Source is TView).Select(x => new ViewNav<TView>(x, (TView)x.Source));

        public static Rx.IObservable<ViewNav<TView>> To<TView>(this Rx.IObservable<Nav> This) where TView : IView =>
            This.Where(x => x.Target is TView).Select(x => new ViewNav<TView>(x, (TView) x.Target));

        public static Rx.IObservable<Nav> Between<TSource, TTarget>(this Rx.IObservable<Nav> This) =>
            This.Where(x => x.Source is TSource && x.Target is TTarget);

        #endregion
    }
}