using System;
using UniRx;

namespace Silphid.Showzup
{
    public static class IPresenterExtensions
    {
        public static IObservable<TView> PresentView<TView>(this IPresenter This) where TView : IView =>
            This.Present(typeof(TView))
                .Cast<IView, TView>();

        public static IObservable<IView> PresentViewModel<TViewModel>(this IPresenter This)
            where TViewModel : IViewModel =>
            This.Present(typeof(TViewModel));

        public static IObservable<IView> TryPresent(this IPresenter This, object input, IOptions options = null) =>
            This.IsReady()
                ? This.Present(input, options)
                : Observable.Empty<IView>();

        public static bool IsReady(this IPresenter This) =>
            This.State.Value == PresenterState.Ready;

        public static bool IsLoading(this IPresenter This) =>
            This.State.Value == PresenterState.Loading;

        public static bool IsPresenting(this IPresenter This) =>
            This.State.Value == PresenterState.Presenting;

        public static IObservable<bool> Ready(this IPresenter This) =>
            This.State.Select(x => x == PresenterState.Ready);

        public static IObservable<bool> Loading(this IPresenter This) =>
            This.State.Select(x => x == PresenterState.Loading);

        public static IObservable<bool> Presenting(this IPresenter This) =>
            This.State.Select(x => x == PresenterState.Presenting);
    }
}