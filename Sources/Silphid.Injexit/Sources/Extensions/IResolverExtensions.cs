using System;

namespace Silphid.Injexit
{
    public static class IResolverExtensions
    {
        public static IResolver With(this IResolver This, Action<IBinder> bind)
        {
            var overrideContainer = new Container();
            bind(overrideContainer);
            return new CompositeResolver(overrideContainer, This);
        }

        public static IResolver With(this IResolver This, IResolver overrideResolver) =>
            overrideResolver != null
                ? new CompositeResolver(overrideResolver, This)
                : This;

        public static object ResolveInstance(this IResolver This, Type abstractionType, bool isOptional = false, bool isFallbackToSelfBinding = true) =>
            This.Resolve(abstractionType, isOptional, isFallbackToSelfBinding)
                ?.Invoke(This);

        public static T ResolveInstance<T>(this IResolver This, bool isOptional = false, bool isFallbackToSelfBinding = true) =>
            (T) This.ResolveInstance(typeof(T), isOptional, isFallbackToSelfBinding);
    }
}