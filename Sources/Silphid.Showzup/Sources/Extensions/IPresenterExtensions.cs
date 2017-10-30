using System;
using UniRx;

namespace Silphid.Showzup
{
    public static class IPresenterExtensions
    {
        public static IObservable<TView> PresentView<TView>(this IPresenter This) where TView : IView =>
            This.Present(typeof(TView))
                .Cast<IView, TView>();

        public static IObservable<IView> PresentViewModel<TViewModel>(this IPresenter This) where TViewModel : IViewModel =>
            This.Present(typeof(TViewModel));
        
        public static IPresenter With(this IPresenter This, PushMode pushMode) =>
            new PushModePresenterDecorator(This, pushMode);

        public static IPresenter With(this IPresenter This, Direction direction) =>
            new DirectionPresenterDecorator(This, direction);

        public static IPresenter With(this IPresenter This, VariantSet variants) =>
            new VariantsPresenterDecorator(This, variants);

        public static IPresenter With(this IPresenter This, params IVariant[] variants) =>
            new VariantsPresenterDecorator(This, variants.ToVariantSet());

        public static IPresenter With(this IPresenter This, ITransition transition) =>
            new TransitionPresenterDecorator(This, transition);

        public static IPresenter With(this IPresenter This, Func<object, Options, ITransition> transition) =>
            new TransitionSelectorPresenterDecorator(This, transition);

        public static IPresenter With(this IPresenter This, Func<object, ITransition> transition) =>
            new TransitionSelectorPresenterDecorator(This, (obj, options) => transition(obj));

        public static IPresenter WithTransitionForInputOfType<T>(this IPresenter This, ITransition transition) =>
            new TransitionSelectorPresenterDecorator(This, (obj, options) => obj is T ? transition : null);

        public static IPresenter WithDuration(this IPresenter This, float duration) =>
            new TransitionDurationPresenterDecorator(This, duration);

        public static IPresenter WithParameters(this IPresenter This, params object[] instances) =>
            new ParametersPresenterDecorator(This, instances);

        public static IPresenter WithParameter<T>(this IPresenter This, T instance) =>
            new TypedParameterPresenterDecorator(This, typeof(T), instance);

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