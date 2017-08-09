using System;

namespace Silphid.Injexit
{
    public interface IReflector
    {
        InjectTypeInfo GetTypeInfo(Type type);
    }
}