using System;

namespace Silphid.Injexit
{
    public interface IBinder
    {
        /// <summary>
        /// Adds a binding to resolve all requests for a given abstraction type
        /// to a given concretion type.
        /// </summary>
        IBinding Bind(Type abstractionType, Type concretionType);

        /// <summary>
        /// Adds a binding that dynamically resolves instances via given selector.
        /// </summary>
        IBinding<T> Bind<T>(Type abstractionType, Func<IResolver, T> factory);

        /// <summary>
        /// Adds a binding to resolve all requests for a given abstraction type
        /// to a given object instance.
        /// </summary>
        IBinding BindInstance(Type abstractionType, object instance);

        /// <summary>
        /// Binds binding with given Name to an extra abstraction type.
        /// </summary>
        IBinding BindReference(Type sourceAbstractionType, BindingId id);
    }
}