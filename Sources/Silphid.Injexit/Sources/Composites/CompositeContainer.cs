using System;
using System.Collections.Generic;
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

        public Func<IResolver, object> ResolveFactory(Type abstractionType, string id = null, bool isOptional = false) =>
            _containers
                .Select((index, x) => x.ResolveFactory(abstractionType, id, true))
                .FirstNotNullOrDefault()
            ?? ThrowIfNotOptional(abstractionType, isOptional);

        private Func<IResolver, object> ThrowIfNotOptional(Type abstractionType, bool isOptional)
        {
            if (!isOptional)
                throw new Exception($"No mapping for required type {abstractionType.Name}.");

            return null;
        }

        #endregion

        #region IBinder members

        public IBinding Bind(Type abstractionType, Type concretionType) =>
            _containers
                .First()
                .Bind(abstractionType, concretionType);

        public IBinding BindInstance(Type abstractionType, object instance) =>
            _containers
                .First()
                .BindInstance(abstractionType, instance);

        public void BindForward(Type sourceAbstractionType, Type targetAbstractionType) =>
            _containers
                .First()
                .BindForward(sourceAbstractionType, targetAbstractionType);

        #endregion

        #region IInjector members

        public void Inject(object obj, IResolver resolver = null) =>
            _containers
                .First()
                .Inject(obj, resolver ?? this);

        public void Inject(IEnumerable<object> objects, IResolver resolver = null) =>
            _containers
                .First()
                .Inject(objects, resolver);

        public IContainer Create() =>
            _containers
                .First()
                .Create();

        #endregion

        #region IDisposable members

        public void Dispose() =>
            _containers
                .First()
                .Dispose();

        #endregion
    }
}