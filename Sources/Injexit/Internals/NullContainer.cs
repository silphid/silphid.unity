using System;
using System.Collections.Generic;

namespace Silphid.Injexit
{
    internal class NullContainer : IContainer
    {
        public IContainer Create() => Container.Null;

        public IBinding Bind(Type abstractionType, Type concretionType) => Binding.Null;
        public IBinding<T> Bind<T>(Type abstractionType, Func<IResolver, T> factory) => Binding.Null.Typed<T>();
        public IBinding BindInstance(Type abstractionType, object instance) => Binding.Null;
        public IBinding BindReference(Type sourceAbstractionType, BindingId id) => Binding.Null;

        public Result ResolveResult(Type abstractionType, Type dependentType = null, string name = null) =>
            new Result(_ => null, null);

        public IResolver BaseResolver => Container.Null;

        public void Inject(object obj, IResolver overrideResolver = null) {}
        public void Inject(IEnumerable<object> objects, IResolver overrideResolver = null) {}

        public void Dispose() {}
        public void InstantiateEagerSingles(IResolver resolver) {}
    }
}