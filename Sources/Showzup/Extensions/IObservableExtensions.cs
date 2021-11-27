using System;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Showzup
{
    public static class IObservableExtensions
    {
        #region IObservable<Nav>

        public static IObservable<ViewNavPresentation<TView>> From<TView>(this IObservable<NavPresentation> This)
            where TView : IView =>
            This.Where(x => x.Source is TView)
                .Select(x => new ViewNavPresentation<TView>(x, (TView) x.Source));

        public static IObservable<ViewNavPresentation<TView>> To<TView>(this IObservable<NavPresentation> This)
            where TView : IView =>
            This.Where(x => x.Target is TView)
                .Select(x => new ViewNavPresentation<TView>(x, (TView) x.Target));

        public static IObservable<NavPresentation> Between<TSource, TTarget>(this IObservable<NavPresentation> This) =>
            This.Where(x => x.Source is TSource && x.Target is TTarget);

        #endregion

        #region AsViewModel/Model

        public static IObservable<TViewModel> AsViewModel<TViewModel>(this IObservable<IView> This)
            where TViewModel : IViewModel =>
            This.OfType<IView, View<TViewModel>>()
                .Select(v => v.ViewModel);

        public static IObservable<TModel> AsModel<TModel>(this IObservable<IViewModel> This) =>
            This.OfType<IViewModel, ViewModel<TModel>>()
                .Select(v => v.Model);

        #endregion

        #region Binding

        public static IDisposable BindTo<T>(this IObservable<T> This, IPresenter target) =>
            This.Subscribe(
                x => target.Present(x)
                           .SubscribeAndForget());

        #endregion
    }
}