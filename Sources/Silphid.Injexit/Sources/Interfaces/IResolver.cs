using System;

namespace Silphid.Injexit
{
    public interface IResolver
    {
        Func<IResolver, object> Resolve(
            Type abstractionType,
            bool isOptional = false,
            bool isFallbackToSelfBinding = true);
    }
}