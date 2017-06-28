using System;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public class CompositeContainer : IContainer
    {
        private readonly IContainer[] _containers;

        public CompositeContainer(params IContainer[] containers)
        {
            _containers = containers
                .WhereNotNull()
                .ToArray();
        }

        #region IResolver members

        public Func<IResolver, object> Resolve(Type abstractionType, bool isOptional = false, bool isFallbackToSelfBinding = true) =>
            _containers
                .Select((index, x) => x.Resolve(abstractionType, true, IsSelfBindingAllowed(index, isFallbackToSelfBinding)))
                .FirstNotNullOrDefault()
            ?? ThrowIfNotOptional(abstractionType, isOptional);

        private bool IsSelfBindingAllowed(int index, bool isSelfBindingAllowed) =>
            index == _containers.Length - 1 && isSelfBindingAllowed; 
        
        private Func<IResolver, object> ThrowIfNotOptional(Type abstractionType, bool isOptional)
        {
            if (!isOptional)
                throw new Exception($"No mapping for required type {abstractionType.Name}.");

            return null;
        }

        #endregion

        #region IBinder members

        public IBinding Bind(Type abstractionType, Type concretionType)
        {
            throw new NotSupportedException("CompositeContainer cannot be added extra bindings.");
        }

        public IBinding BindInstance(Type abstractionType, object instance)
        {
            throw new NotSupportedException("CompositeContainer cannot be added extra bindings.");
        }

        #endregion

        #region IInjector members

        public void Inject(object obj, IResolver overrideResolver = null) =>
            _containers
                .First()
                .Inject(obj, this);

        public IContainer CreateChild() =>
            _containers
                .First()
                .CreateChild();

        #endregion
    }
}