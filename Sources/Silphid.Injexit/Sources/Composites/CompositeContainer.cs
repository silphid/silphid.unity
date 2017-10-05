using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public class CompositeContainer : IContainer
    {
        private readonly IContainer[] _containers;
        private int _recursionDepth;

        public CompositeContainer(params IContainer[] containers)
        {
            _containers = containers
                .WhereNotNull()
                .ToArray();
        }

        #region IContainer members

        public IContainer Create() =>
            _containers
                .First()
                .Create();

        #endregion
        
        #region IResolver members

        public Func<IResolver, object> ResolveFactory(Type abstractionType, string name = null)
        {
            _recursionDepth++;
            
            try
            {
                if (_recursionDepth > Container.MaxRecursionDepth)
                    throw new CircularDependencyException(abstractionType);

                try
                {
                    foreach (var container in _containers)
                    {
                        try
                        {
                            return container.ResolveFactory(abstractionType, name);
                        }
                        catch (UnresolvedTypeException)
                        {
                        }
                    }
            
                    throw new UnresolvedTypeException(abstractionType, name);
                }
                catch (CircularDependencyException ex)
                {
                    throw new CircularDependencyException(abstractionType, ex);
                }
            }
            finally
            {
                _recursionDepth--;
            }
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

        public IBinding BindReference(Type sourceAbstractionType, BindingId id) =>
            _containers
                .First()
                .BindReference(sourceAbstractionType, id);

        #endregion

        #region IInjector members

        public void Inject(object obj, IResolver resolver = null) =>
            _containers
                .First()
                .Inject(obj, resolver ?? this);

        public void Inject(IEnumerable<object> objects, IResolver resolver = null) =>
            _containers
                .First()
                .Inject(objects, resolver ?? this);

        #endregion

        #region IDisposable members

        public void Dispose() =>
            _containers
                .First()
                .Dispose();

        public void InstantiateEagerSingles() =>
            _containers.ForEach(x => x.InstantiateEagerSingles());

        #endregion
    }
}