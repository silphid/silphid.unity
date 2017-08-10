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
        
        public static IBinding UsingInstance<T>(this IBinding This, T instance) =>
            This.Using(x => x.BindInstance(instance));
        
        public static IBinding Using<TAbstraction, TConcretion>(this IBinding This) where TConcretion : TAbstraction =>
            This.Using(x => x.Bind<TAbstraction, TConcretion>());

        public static IBinding WithId(this IBinding This, string id) =>
            This.WithId(id);
    }
}