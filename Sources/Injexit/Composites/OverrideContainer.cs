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

        public Result ResolveResult(Type abstractionType, Type dependentType = null, string name = null)
        {
            try
            {
                _recursionDepth++;
                if (_recursionDepth > Container.MaxRecursionDepth)
                    throw new CircularDependencyException(abstractionType);

                try
                {
                    var result = _overrideContainer.ResolveResult(abstractionType, dependentType, name);

                    if (result.Exception != null)
                        result = _baseContainer.ResolveResult(abstractionType, dependentType, name);

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

        public IBinding<T> Bind<T>(Type abstractionType, Func<IResolver, T> selector) =>
            _overrideContainer.Bind(abstractionType, selector);

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

        public void InstantiateEagerSingles(IResolver resolver)
        {
            _baseContainer.InstantiateEagerSingles(resolver);
            _overrideContainer.InstantiateEagerSingles(resolver);
        }

        #endregion

        public override string ToString() =>
            $"{_overrideContainer}\r\n" + "----------\r\n" + $"{_baseContainer}";
    }
}