using System;

namespace Silphid.Injexit
{
    public static class IResolverExtensions
    {
        public static IResolver Using(this IResolver This, Action<IBinder> bind)
        {
            var overrideContainer = This.Create();
            bind(overrideContainer);
            return new CompositeResolver(overrideContainer, This);
        }

        public static IResolver Using(this IResolver This, IResolver overrideResolver) =>
            overrideResolver != null
                ? new CompositeResolver(overrideResolver, This)
                : This;

        public static object Resolve(this IResolver This, Type abstractionType, string id = null) =>
            This.ResolveFactory(abstractionType, id)
                ?.Invoke(This);

        public static T Resolve<T>(this IResolver This, string id = null) =>
            (T) This.Resolve(typeof(T), id);
    }
}