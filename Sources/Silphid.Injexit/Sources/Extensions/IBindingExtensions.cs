using System;

namespace Silphid.Injexit
{
    public static class IBindingExtensions
    {
        public static IBinding With(this IBinding This, Action<IBinder> bind)
        {
            var child = This.Container.CreateChild();
            bind(child);
            return This.With(child);
        }
    }
}