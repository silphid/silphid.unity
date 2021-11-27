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
        /// <param name="dependentType">Type that depends on the abstraction to be resolved</param>
        /// <param name="name">Name of dependency member that will be assigned the resolved</param>
        Result ResolveResult(Type abstractionType, Type dependentType = null, string name = null);

        IResolver BaseResolver { get; }
    }
}