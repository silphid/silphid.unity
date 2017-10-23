using System;

namespace Silphid.Injexit
{
    public interface IResolver
    {
        /// <summary>
        /// Creates container with same configs as this.
        /// </summary>
        IContainer Create();

        /// <summary>
        /// Resolves given abstraction type to a factory able to create
        /// concrete objects of that type.
        /// </summary>
        /// <param name="abstractionType">Abstraction type that object must derive from or implement (if it's an interface)</param>
        /// <param name="name">Optional Named to match on binding (if not specified/null, binding must not have any Named associated with it in order to match)</param>
        Result ResolveResult(
            Type abstractionType,
            string name = null);
        
        IResolver BaseResolver { get; }
    }
}