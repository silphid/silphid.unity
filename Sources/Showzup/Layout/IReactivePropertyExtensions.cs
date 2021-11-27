using System;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Showzup.Layout
{
    public static class IReactivePropertyExtensions
    {
        public static IDisposable BindTo(this IReactiveProperty<float> This,
                                         IObservable<float> source,
                                         float offset = 0)
        {
            var isChanging = false;
            return source.Subscribe(
                x =>
                {
                    if (isChanging)
                        return;

                    isChanging = true;
                    This.Value = x + offset;
                    isChanging = false;
                });
        }

        public static IDisposable BindTwoWayTo(this IReactiveProperty<float> This,
                                               IReactiveProperty<float> source,
                                               float offset = 0)
        {
            var isChanging = false;
            return new CompositeDisposable(
                source.Subscribe(
                    x =>
                    {
                        if (isChanging)
                            return;

                        isChanging = true;
                        This.Value = x + offset;
                        isChanging = false;
                    }),
                This.Subscribe(
                    x =>
                    {
                        if (isChanging)
                            return;

                        isChanging = true;
                        source.Value = x - offset;
                        isChanging = false;
                    }));
        }

        public static IDisposable BindThreeWayTo(this IReactiveProperty<float> This,
                                                 IReactiveProperty<float> dependentSource,
                                                 IObservable<float> independentSource,
                                                 Func<float, float, float> targetSelector,
                                                 Func<float, float, float> dependentSelector)
        {
            var isChanging = false;
            return new CompositeDisposable(
                dependentSource.CombineLatest(independentSource)
                              .Subscribe(
                                   x =>
                                   {
                                       if (isChanging)
                                           return;

                                       isChanging = true;
                                       var (item1, item2) = x;
                                       This.Value = targetSelector(item1, item2);
                                       isChanging = false;
                                   }),
                This.CombineLatest(independentSource)
                              .Subscribe(
                                   x =>
                                   {
                                       if (isChanging)
                                           return;

                                       isChanging = true;
                                       dependentSource.Value = dependentSelector(x.Item1, x.Item2);
                                       isChanging = false;
                                   }));
        }
    }
}