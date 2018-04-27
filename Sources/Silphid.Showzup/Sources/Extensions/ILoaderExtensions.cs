using Silphid.Loadzup;
using UnityEngine;

namespace Silphid.Showzup
{
    public static class ILoaderExtensions
    {
        public static ILoader WithCancellationOnDestroy(this ILoader This, GameObject gameObject) =>
            new CancellationOnDestroyLoaderDecorator(This, gameObject);
        
        public static ILoader WithCancellationOnDestroy(this ILoader This, Component component) =>
            This.WithCancellationOnDestroy(component.gameObject);
        
        public static ILoader WithCancellationOnDestroy(this ILoader This, IView view) =>
            This.WithCancellationOnDestroy(view.GameObject);
    }
}