using Silphid.Loadzup;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Silphid.Showzup
{
    public static class ILoaderExtensions
    {
        public static ILoader WithCancellationOnDestroy(this ILoader This, GameObject gameObject)
        {
            var cancellationToken = new CancellationDisposable();

            gameObject.OnDestroyAsObservable()
                      .Take(1)
                      .Subscribe(_ => cancellationToken.Dispose());

            return This.With(cancellationToken);
        }

        public static ILoader WithCancellationOnDestroy(this ILoader This, Component component) =>
            This.WithCancellationOnDestroy(component.gameObject);

        public static ILoader WithCancellationOnDestroy(this ILoader This, IView view) =>
            This.WithCancellationOnDestroy(view.GameObject);
    }
}