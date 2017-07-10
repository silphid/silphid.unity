using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;

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

        public IBinding Bind(Type abstractionType, Type concretionType)
        {
            throw new NotSupportedException("CompositeContainer cannot be added extra bindings.");
        }

        public IBinding BindInstance(Type abstractionType, object instance)
        {
            throw new NotSupportedException("CompositeContainer cannot be added extra bindings.");
        }

        public void BindForward(Type sourceAbstractionType, Type targetAbstractionType)
        {
            throw new NotSupportedException("CompositeContainer cannot be added extra forward bindings.");
        }

        #endregion

        #region IInjector members

        public void Inject(object obj, IResolver overrideResolver = null) =>
            _containers
                .First()
                .Inject(obj, this);

        public void InjectGameObjects(IEnumerable<GameObject> gameObjects) =>
            _containers
                .First()
                .InjectGameObjects(gameObjects);

        public IContainer CreateChild() =>
            _containers
                .First()
                .CreateChild();

        #endregion

        public Func<IResolver, object> ResolveFactory(Type abstractionType, string id = null, bool isOptional = false,
            bool isFallbackToSelfBinding = true)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}