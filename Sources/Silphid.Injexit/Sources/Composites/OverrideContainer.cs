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

        public Result ResolveResult(Type abstractionType, string name = null)
        {
            try
            {
                _recursionDepth++;
                if (_recursionDepth > Container.MaxRecursionDepth)
                    throw new CircularDependencyException(abstractionType);

                try
                {
                    var result = _overrideContainer.ResolveResult(abstractionType, name);
                    
                    if (result.Exception is UnresolvedTypeException)
                        result = _baseContainer.ResolveResult(abstractionType, name);
                    
                    return result;
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

        public override string ToString() =>
            $"Overrides:\r\n{_overrideContainer}\r\n" +
            $"Base:\r\n{_baseContainer}";
    }
}