using System;

namespace Silphid.Injexit
{
    public static class IResolverExtensions
    {
        public static IResolver Using(this IResolver This, Action<IBinder> bind)
        {
            var overrideContainer = This.Create();
            bind(overrideContainer);
            return new OverrideResolver(This, overrideContainer, false);
        }

        public static IResolver Using(this IResolver This, IResolver overrideResolver) =>
            overrideResolver != null
                ? new OverrideResolver(This, overrideResolver, false)
                : This;
        
        public static IResolver UsingInstance<T>(this IResolver This, T instance) =>
            This.Using(x => x.BindInstance(instance));
        
        public static IResolver UsingInstances<T1, T2>(this IResolver This, T1 instance1, T2 instance2) =>
            This.Using(x =>
            {
                x.BindInstance(instance1);
                x.BindInstance(instance2);
            });
        
        public static IResolver UsingInstances<T1, T2, T3>(this IResolver This, T1 instance1, T2 instance2, T3 instance3) =>
            This.Using(x =>
            {
                x.BindInstance(instance1);
                x.BindInstance(instance2);
                x.BindInstance(instance3);
            });
        
        public static IResolver UsingInstances(this IResolver This, object[] instances) =>
            This.Using(x => x.BindInstances(instances));

        public static object Resolve(this IResolver This, Type abstractionType, string name = null) =>
            This.ResolveFactory(abstractionType, name).Invoke(This.BaseResolver);

        public static T Resolve<T>(this IResolver This, string id = null) =>
            (T) This.Resolve(typeof(T), id);
    }
}