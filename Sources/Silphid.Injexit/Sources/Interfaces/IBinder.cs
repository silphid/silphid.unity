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
        /// Adds a binding to resolve all requests for a given abstraction type
        /// to a given object instance.
        /// </summary>
        IBinding BindInstance(Type abstractionType, object instance);
        
        /// <summary>
        /// Adds a binding to forward all requests for a given abstraction type
        /// to another abstraction type binding. The target abstraction type must
        /// therefore have its own binding to some concretion type. This is useful
        /// when an object implements multiple interfaces (only configure a binding
        /// for one of its interfaces, and forward all others to first one).
        /// </summary>
        void BindForward(Type sourceAbstractionType, Type targetAbstractionType);
    }
}