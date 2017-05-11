using System.Collections.Generic;
using JetBrains.Annotations;
using UniRx;

namespace Silphid.Showzup
{
    public interface INavigationPresenter : IPresenter
    {
        ReadOnlyReactiveProperty<bool> IsNavigating { get; }
        ReadOnlyReactiveProperty<bool> IsLoading { get; }
        ReadOnlyReactiveProperty<bool> CanPresent { get; }
        ReadOnlyReactiveProperty<bool> CanPop { get; }
        ReadOnlyReactiveProperty<IView> View { get; }
        IObservable<Nav> Navigating { get; }
        IObservable<Nav> Navigated { get; }
        ReactiveProperty<List<IView>> History { get; }

        [Pure] IObservable<IView> Pop();
        [Pure] IObservable<IView> PopToRoot();
        [Pure] IObservable<IView> PopTo(IView view);
    }
}