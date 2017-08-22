using System.Collections.Generic;

namespace Silphid.Showzup
{
    public static class IPresenterExtensions
    {
        public static IPresenter With(this IPresenter This, PushMode pushMode) =>
            new PushModePresenterDecorator(This, pushMode);

        public static IPresenter With(this IPresenter This, Direction direction) =>
            new DirectionPresenterDecorator(This, direction);

        public static IPresenter With(this IPresenter This, VariantSet variants) =>
            new VariantsPresenterDecorator(This, variants);

        public static IPresenter With(this IPresenter This, params IVariant[] variants) =>
            new VariantsPresenterDecorator(This, variants.ToVariantSet());

        public static IPresenter WithDuration(this IPresenter This, float duration) =>
            new TransitionDurationPresenterDecorator(This, duration);

        public static IPresenter WithParameters(this IPresenter This, IEnumerable<object> parameters) =>
            new ParametersPresenterDecorator(This, parameters);

        public static IPresenter WithParameters(this IPresenter This, params object[] parameters) =>
            new ParametersPresenterDecorator(This, parameters);
    }
}