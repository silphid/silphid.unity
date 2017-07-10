using System;

namespace Silphid.Injexit
{
    public interface IResolver
    {
        Func<IResolver, object> ResolveFactory(
            Type abstractionType,
            string id = null,
            bool isOptional = false,
            bool isFallbackToSelfBinding = true);
    }
}