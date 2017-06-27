using System;

namespace Silphid.Showzup.Injection
{
    public interface IBinder
    {
        void BindInstance(Type abstractionType, object instance);
        void BindInstanceAsList(Type abstractionType, object instance);
        void Bind(Type abstractionType, Type concretionType, IResolver overrideResolver = null);
        void BindAsList(Type abstractionType, Type concretionType, IResolver overrideResolver = null);
        void BindSingle(Type abstractionType, Type concretionType, IResolver overrideResolver = null);
        void BindSingleAsList(Type abstractionType, Type concretionType, IResolver overrideResolver = null);
    }
}