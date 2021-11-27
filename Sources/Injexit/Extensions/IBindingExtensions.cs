using System;

namespace Silphid.Injexit
{
    public static class IBindingExtensions
    {
        public static IBinding<T> Typed<T>(this IBinding This) => new TypedBinding<T>(This);

        public static IBinding Using(this IBinding This, Action<IBinder> bind)
        {
            var child = This.Container.Create();
            bind(child);
            return This.Using(child);
        }

        public static IBinding<TAbstraction> Using<TAbstraction>(this IBinding<TAbstraction> This, Action<IBinder> bind)
        {
            var child = This.Container.Create();
            bind(child);
            return This.Using(child);
        }

        public static IBinding Using<TAbstraction, TConcretion>(this IBinding This) where TConcretion : TAbstraction =>
            This.Using(x => x.Bind<TAbstraction, TConcretion>());

        public static IBinding<TOuterAbstraction>
            Using<TOuterAbstraction, TInnerAbstraction, TInnerConcretion>(this IBinding<TOuterAbstraction> This)
            where TInnerConcretion : TInnerAbstraction =>
            This.Using(x => x.Bind<TInnerAbstraction, TInnerConcretion>())
                .Typed<TOuterAbstraction>();

        public static IBinding UsingRecursively(this IBinding This, Action<IBinder> bind)
        {
            var child = This.Container.Create();
            bind(child);
            return This.UsingRecursively(child);
        }

        public static IBinding UsingInstance<T>(this IBinding This, T instance) =>
            This.Using(x => x.BindInstance(instance));

        public static IBinding UsingInstances<T1, T2>(this IBinding This, T1 instance1, T2 instance2) =>
            This.Using(
                x =>
                {
                    x.BindInstance(instance1);
                    x.BindInstance(instance2);
                });

        public static IBinding
            UsingInstances<T1, T2, T3>(this IBinding This, T1 instance1, T2 instance2, T3 instance3) =>
            This.Using(
                x =>
                {
                    x.BindInstance(instance1);
                    x.BindInstance(instance2);
                    x.BindInstance(instance3);
                });

        public static IBinding UsingOptionalInstance<T>(this IBinding This, T instance) =>
            This.Using(x => x.BindOptionalInstance(instance));

        public static IBinding UsingOptionalInstances<T1, T2>(this IBinding This, T1 instance1, T2 instance2) =>
            This.Using(
                x =>
                {
                    x.BindOptionalInstance(instance1);
                    x.BindOptionalInstance(instance2);
                });

        public static IBinding UsingOptionalInstances<T1, T2, T3>(this IBinding This,
                                                                  T1 instance1,
                                                                  T2 instance2,
                                                                  T3 instance3) =>
            This.Using(
                x =>
                {
                    x.BindOptionalInstance(instance1);
                    x.BindOptionalInstance(instance2);
                    x.BindOptionalInstance(instance3);
                });

        public static IBinding UsingSelf<TConcretion>(this IBinding This) =>
            This.Using(x => x.BindToSelf<TConcretion>());

        public static IBinding Named(this IBinding This, string id) =>
            This.Named(id);

        public static IBinding<TAbstraction> Alias<TAbstraction>(this IBinding This) =>
            This.Alias(typeof(TAbstraction))
                .Typed<TAbstraction>();

        public static IBinding<T> WithDecoration<T>(this IBinding<T> This, Func<T, T> decoration) =>
            This.WithDecoration((_, inner) => decoration(inner));
    }
}