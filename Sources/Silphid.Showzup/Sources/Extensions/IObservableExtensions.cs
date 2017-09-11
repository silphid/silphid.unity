using System;
using Silphid.Extensions;
using Silphid.Requests;
using Silphid.Showzup.Requests;
using UniRx;
using UnityEngine.UI;

namespace Silphid.Showzup
{
    public static class IObservableExtensions
    {
        #region IObservable<object>

        public static IDisposable BindTo(this IObservable<object> This, IPresenter target) =>
            This.Subscribe(x => target.Present(x).SubscribeAndForget());

        public static IDisposable BindTo(this Button This, IRequest request) =>
            This.OnClickAsObservable().Subscribe(_ => This.Send(request));

        public static IDisposable BindTo<TRequest>(this Button This) where TRequest : IRequest, new() =>
            This.OnClickAsObservable().Subscribe(_ => This.Send<TRequest>());

        #endregion

        #region IObservable<Nav>

        public static IObservable<ViewNav<TView>> From<TView>(this IObservable<Nav> This) where TView : IView =>
            This.Where(x => x.Source is TView).Select(x => new ViewNav<TView>(x, (TView)x.Source));

        public static IObservable<ViewNav<TView>> To<TView>(this IObservable<Nav> This) where TView : IView =>
            This.Where(x => x.Target is TView).Select(x => new ViewNav<TView>(x, (TView) x.Target));

        public static IObservable<Nav> Between<TSource, TTarget>(this IObservable<Nav> This) =>
            This.Where(x => x.Source is TSource && x.Target is TTarget);

        #endregion

        #region AsViewModel/Model

        public static IObservable<TViewModel> AsViewModel<TViewModel>(this IObservable<IView> This) where TViewModel : IViewModel =>
            This.OfType<IView, View<TViewModel>>()
                .Select(v => v.ViewModel);

        public static IObservable<TModel> AsModel<TModel>(this IObservable<IViewModel> This) =>
            This.OfType<IViewModel, ViewModel<TModel>>()
                .Select(v => v.Model);

        #endregion
    }
}