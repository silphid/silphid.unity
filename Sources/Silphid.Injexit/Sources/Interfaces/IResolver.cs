using System;

namespace Silphid.Showzup.Injection
{
    public interface IResolver
    {
        Func<IResolver, object> ResolveFactory(
            Type abstractionType,
            bool isOptional = false,
            bool isFallbackToSelfBinding = true);
    }
}