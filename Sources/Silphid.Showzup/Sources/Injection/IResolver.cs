using System;

namespace Silphid.Showzup.Injection
{
    public interface IResolver
    {
        object Resolve(Type abstractionType, IResolver subResolver = null, bool isOptional = false);
    }
}