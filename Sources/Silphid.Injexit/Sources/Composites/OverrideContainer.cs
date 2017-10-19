using System;
using System.Collections.Generic;

namespace Silphid.Injexit
{
    public class OverrideContainer : IContainer
    {
        private readonly IContainer _baseContainer;
        private readonly IContainer _overrideContainer;
        private readonly bool _isRecursive;
        private int _recursionDepth;

        public OverrideContainer(IContainer baseContainer, IContainer overrideContainer, bool isRecursive)
        {
            _baseContainer = baseContainer;
            _overrideContainer = overrideContainer;
            _isRecursive = isRecursive;
        }

        #region IContainer members

        public IContainer Create() =>
            _overrideContainer.Create();

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
                    try
                    {
                        return _overrideContainer.ResolveFactory(abstractionType, name);
                    }
                    catch (UnresolvedTypeException)
                    {
                        return _baseContainer.ResolveFactory(abstractionType, name);
                    }
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

        public IResolver BaseResolver =>
            _isRecursive
                ? this
                : _baseContainer.BaseResolver;

        #endregion

        #region IBinder members

        public IBinding Bind(Type abstractionType, Type concretionType) =>
            _overrideContainer.Bind(abstractionType, concretionType);

        public IBinding BindInstance(Type abstractionType, object instance) =>
            _overrideContainer.BindInstance(abstractionType, instance);

        public IBinding BindReference(Type sourceAbstractionType, BindingId id) =>
            _overrideContainer.BindReference(sourceAbstractionType, id);

        #endregion

        #region IInjector members

        public void Inject(object obj, IResolver resolver = null) =>
            _overrideContainer.Inject(obj, resolver ?? this);

        public void Inject(IEnumerable<object> objects, IResolver resolver = null) =>
            _overrideContainer.Inject(objects, resolver ?? this);

        #endregion

        #region IDisposable members

        public void Dispose() =>
            _overrideContainer.Dispose();

        public void InstantiateEagerSingles()
        {
            _baseContainer.InstantiateEagerSingles();
            _overrideContainer.InstantiateEagerSingles();
        }

        #endregion
    }
}