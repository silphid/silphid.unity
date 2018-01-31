using System;

namespace Silphid.Injexit
{
    public static class IBindingExtensions
    {
        public static IBinding Using(this IBinding This, Action<IBinder> bind)
        {
            var child = This.Container.Create();
            bind(child);
            return This.Using(child);
        }
        
        public static IBinding UsingRecursively(this IBinding This, Action<IBinder> bind)
        {
            var child = This.Container.Create();
            bind(child);
            return This.UsingRecursively(child);
        }
        
        public static IBinding UsingInstance<T>(this IBinding This, T instance) =>
            This.Using(x => x.BindInstance(instance));
        
        public static IBinding UsingInstances<T1, T2>(this IBinding This, T1 instance1, T2 instance2) =>
            This.Using(x =>
            {
                x.BindInstance(instance1);
                x.BindInstance(instance2);
            });
        
        public static IBinding UsingInstances<T1, T2, T3>(this IBinding This, T1 instance1, T2 instance2, T3 instance3) =>
            This.Using(x =>
            {
                x.BindInstance(instance1);
                x.BindInstance(instance2);
                x.BindInstance(instance3);
            });
        
        public static IBinding UsingOptionalInstance<T>(this IBinding This, T instance) =>
            This.Using(x => x.BindOptionalInstance(instance));
        
        public static IBinding UsingOptionalInstances<T1, T2>(this IBinding This, T1 instance1, T2 instance2) =>
            This.Using(x =>
            {
                x.BindOptionalInstance(instance1);
                x.BindOptionalInstance(instance2);
            });
        
        public static IBinding UsingOptionalInstances<T1, T2, T3>(this IBinding This, T1 instance1, T2 instance2, T3 instance3) =>
            This.Using(x =>
            {
                x.BindOptionalInstance(instance1);
                x.BindOptionalInstance(instance2);
                x.BindOptionalInstance(instance3);
            });
        
        public static IBinding UsingSelf<TConcretion>(this IBinding This) =>
            This.Using(x => x.BindToSelf<TConcretion>());
        
        public static IBinding Using<TAbstraction, TConcretion>(this IBinding This) where TConcretion : TAbstraction =>
            This.Using(x => x.Bind<TAbstraction, TConcretion>());

        public static IBinding Named(this IBinding This, string id) =>
            This.Named(id);

        public static IBinding Alias<TAbstraction>(this IBinding This) =>
            This.Alias(typeof(TAbstraction));
    }
}