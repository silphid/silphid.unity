using System;
using System.Collections.Generic;

namespace Silphid.Injexit
{
    internal class NullContainer : IContainer
    {
        public IContainer Create() => Container.Null;

        public IBinding Bind(Type abstractionType, Type concretionType) => Binding.Null;
        public IBinding BindInstance(Type abstractionType, object instance) => Binding.Null;
        public void BindForward(Type sourceAbstractionType, Type targetAbstractionType) {}

        public Func<IResolver, object> ResolveFactory(Type abstractionType, string id = null) => _ => null;
        
        public void Inject(object obj, IResolver overrideResolver = null) {}
        public void Inject(IEnumerable<object> objects, IResolver overrideResolver = null) {}

        public void Dispose() {}
    }
}