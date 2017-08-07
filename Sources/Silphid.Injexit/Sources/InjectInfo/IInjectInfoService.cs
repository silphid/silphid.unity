using System;

namespace Silphid.Injexit
{
    public interface IInjectInfoService
    {
        InjectTypeInfo GetTypeInfo(Type type);
    }
}