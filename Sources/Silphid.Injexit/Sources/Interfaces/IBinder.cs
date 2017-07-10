using System;

namespace Silphid.Injexit
{
    public interface IBinder
    {
        IBinding Bind(Type abstractionType, Type concretionType);
        IBinding BindInstance(Type abstractionType, object instance);
        void BindForward(Type sourceAbstractionType, Type targetAbstractionType);
    }
}