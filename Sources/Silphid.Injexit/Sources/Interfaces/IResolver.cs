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
        /// <param name="id">Optional Id to match on binding (if not specified/null, binding must not have any Id associated with it in order to match)</param>
        /// <param name="isOptional">Whether to return null upon failure (isOptional=true) or throw an exception (isOptional=false)</param>
        Func<IResolver, object> ResolveFactory(
            Type abstractionType,
            string id = null,
            bool isOptional = false);
    }
}