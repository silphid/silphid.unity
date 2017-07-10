using System;

namespace Silphid.Injexit
{
    public static class IBindingExtensions
    {
        public static IBinding Using(this IBinding This, Action<IBinder> bind)
        {
            var child = This.Container.CreateChild();
            bind(child);
            return This.Using(child);
        }
        
        public static IBinding WithId(this IBinding This, string id) =>
            This.WithId(id);
    }
}