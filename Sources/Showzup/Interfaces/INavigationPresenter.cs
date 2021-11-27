using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UniRx;

namespace Silphid.Showzup
{
    public interface INavigationPresenter : IPresenter
    {
        ReadOnlyReactiveProperty<bool> CanPresent { get; }
        ReadOnlyReactiveProperty<bool> CanPop { get; }
        ReadOnlyReactiveProperty<IView> View { get; }
        IReadOnlyReactiveProperty<Nav> RootHistory { get; }
        IObservable<NavPresentation> Navigating { get; }
        IObservable<NavPresentation> Navigated { get; }
        ReactiveProperty<List<Nav>> History { get; }

        [Pure]
        IObservable<IView> Pop();

        [Pure]
        IObservable<IView> PopToRoot();

        [Pure]
        IObservable<IView> PopTo(Nav nav);
    }
}