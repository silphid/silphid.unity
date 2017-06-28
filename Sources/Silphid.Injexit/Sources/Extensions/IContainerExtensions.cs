using System;

namespace Silphid.Injexit
{
    public static class IContainerExtensions
    {
        public static IContainer With(this IContainer This, Action<IBinder> bind)
        {
            var overrideContainer = new Container();
            bind(overrideContainer);
            return This.With(overrideContainer);
        }

        public static IContainer With(this IContainer This, IContainer overrideContainer) =>
            overrideContainer != null
                ? new CompositeContainer(overrideContainer, This)
                : This;
        
        public static void Inject(this IContainer This, object obj, Action<IBinder> bind)
        {
            var child = This.CreateChild();
            bind(child);
            This.Inject(obj, child);
        }
    }
}