using System;
using Silphid.Showzup;
using UniRx;

namespace Plugins.Silphid.Showzup.Navigation
{
    public static class INavigationServiceExtensions
    {
        public static IObservable<TView> SelectedView<TView>(this INavigationService This) where TView : class =>
            This.Selection
                .Select(x =>
                    x?.GetComponent<TView>())
                .DistinctUntilChanged();

        public static IObservable<TViewModel> SelectedViewModel<TViewModel>(this INavigationService This) where TViewModel : class =>
            This.Selection
                .Select(x =>
                    x?.GetComponent<IView>()?.ViewModel as TViewModel)
                .DistinctUntilChanged();

        public static IObservable<TModel> SelectedModel<TModel>(this INavigationService This) where TModel : class =>
            This.Selection
                .Select(x =>
                    x?.GetComponent<IView>()?.ViewModel.Model as TModel)
                .DistinctUntilChanged();
    }
}