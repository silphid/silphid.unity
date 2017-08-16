using System;

namespace Silphid.Injexit
{
    public static class IContainerExtensions
    {
        /// <summary>
        /// Creates child container with extra bindings specified by action.
        /// </summary>
        public static IContainer Using(this IContainer This, Action<IBinder> bind)
        {
            var overrideContainer = This.Create();
            bind(overrideContainer);
            return This.Using(overrideContainer);
        }

        /// <summary>
        /// Creates child container with extra bindings specified by overrideContainer.
        /// If overrideContainer is null, this container is returned as is.
        /// </summary>
        public static IContainer Using(this IContainer This, IContainer overrideContainer) =>
            overrideContainer != null
                ? new CompositeContainer(overrideContainer, This)
                : This;
        
        /// <summary>
        /// Injects given object, using extra bindings specified by action.
        /// </summary>
        public static void Inject(this IContainer This, object obj, Action<IBinder> bind) =>
            This.Inject(obj, This.Using(bind));

        /// <summary>
        /// Creates a child container that inherits all of this container's bindings,
        /// to which extra/override bindings can be added.
        /// </summary>
        public static IContainer Child(this IContainer This) =>
            new CompositeContainer(This.Create(), This);
                
        public static IBinding BindDefaultFactory<TAbstraction>(this IContainer This) =>
            This.BindInstance<Func<TAbstraction>>(() => This.Resolve<TAbstraction>());
                
        public static IBinding BindDefaultFactory<T1, TAbstraction>(this IContainer This) =>
            This.BindInstance<Func<T1, TAbstraction>>(t1 => This
                .Using(x => x.BindInstance(t1))
                .Resolve<TAbstraction>());
                
        public static IBinding BindDefaultFactory<T1, T2, TAbstraction>(this IContainer This) =>
            This.BindInstance<Func<T1, T2, TAbstraction>>((t1, t2) => This
                .Using(x =>
                {
                    x.BindInstance(t1);
                    x.BindInstance(t2);
                })
                .Resolve<TAbstraction>());
                
        public static IBinding BindDefaultFactory<T1, T2, T3, TAbstraction>(this IContainer This) =>
            This.BindInstance<Func<T1, T2, T3, TAbstraction>>((t1, t2, t3) => This
                .Using(x =>
                {
                    x.BindInstance(t1);
                    x.BindInstance(t2);
                    x.BindInstance(t3);
                })
                .Resolve<TAbstraction>());
    }
}