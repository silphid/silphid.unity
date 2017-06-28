using System;

namespace Silphid.Injexit
{
    internal class NullContainer : IContainer
    {
        public IBinding Bind(Type abstractionType, Type concretionType) => Binding.Null;
        public IBinding BindInstance(Type abstractionType, object instance) => Binding.Null;
        public Func<IResolver, object> Resolve(Type abstractionType, bool isOptional = false, bool isFallbackToSelfBinding = true) => _ => null;
        public void Inject(object obj, IResolver overrideResolver = null) {}
        public IContainer CreateChild() => Container.Null;
    }
}