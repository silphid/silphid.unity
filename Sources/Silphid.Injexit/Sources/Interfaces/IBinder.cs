using System;

namespace Silphid.Showzup.Injection
{
    public interface IBinder
    {
        IBinding Bind(Type abstractionType, Type concretionType);
        IBinding BindInstance(Type abstractionType, object instance);
    }
}