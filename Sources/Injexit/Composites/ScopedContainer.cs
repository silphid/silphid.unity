using System;
using System.Collections.Generic;

namespace Silphid.Injexit
{
    public class ScopedContainer : IContainer
    {
        private readonly IContainer _inner;
        private readonly IScope _scope;
        private readonly IDictionary<Type, object> _instances = new Dictionary<Type, object>();

        public ScopedContainer(IContainer inner, IScope scope)
        {
            _inner = inner;
            _scope = scope;
        }

        #region IContainer members

        public void InstantiateEagerSingles(IResolver resolver) =>
            _inner.InstantiateEagerSingles(resolver);

        #endregion

        #region IResolver members

        public IContainer Create() =>
            _inner.Create();

        public Result ResolveResult(Type abstractionType, Type dependentType = null, string name = null)
        {
            var result = _inner.ResolveResult(abstractionType, dependentType, name);

            return result.Factory != null && result.Scope == _scope
                       ? new Result(ResolveInstance, null)
                       : result;

            object ResolveInstance(IResolver resolver)
            {
                if (!_instances.TryGetValue(abstractionType, out var instance))
                {
                    instance = result.Factory(resolver);
                    _instances[abstractionType] = instance;
                }

                return instance;
            }
        }

        public IResolver BaseResolver =>
            _inner.BaseResolver;

        #endregion

        #region IBinder members

        public IBinding Bind(Type abstractionType, Type concretionType) =>
            _inner.Bind(abstractionType, concretionType);

        public IBinding<T> Bind<T>(Type abstractionType, Func<IResolver, T> selector) =>
            _inner.Bind(abstractionType, selector);

        public IBinding BindInstance(Type abstractionType, object instance) =>
            _inner.BindInstance(abstractionType, instance);

        public IBinding BindReference(Type sourceAbstractionType, BindingId id) =>
            _inner.BindReference(sourceAbstractionType, id);

        #endregion

        #region IInjector members

        public void Inject(object obj, IResolver resolver = null) =>
            _inner.Inject(obj, resolver ?? this);

        public void Inject(IEnumerable<object> objects, IResolver resolver = null) =>
            _inner.Inject(objects, resolver ?? this);

        #endregion

        #region IDisposable members

        public void Dispose() =>
            _inner.Dispose();

        #endregion

        public override string ToString() =>
            _inner.ToString();
    }
}