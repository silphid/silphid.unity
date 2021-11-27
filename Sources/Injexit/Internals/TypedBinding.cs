using System;

namespace Silphid.Injexit
{
    internal class TypedBinding<T> : IBinding<T>
    {
        private readonly IBinding _inner;

        public TypedBinding(IBinding inner)
        {
            _inner = inner;
        }

        #region IBinding members

        public IContainer Container => _inner.Container;
        IBinding IBinding.InList() => _inner.InList();
        IBinding IBinding.AsSingle() => _inner.AsSingle();
        IBinding IBinding.AsEagerSingle() => _inner.AsEagerSingle();
        IBinding IBinding.Id(BindingId id) => _inner.Id(id);
        public BindingId Id() => _inner.Id();
        IBinding IBinding.Alias(Type aliasAbstractionType) => _inner.Alias(aliasAbstractionType);
        IBinding IBinding.Scoped(IScope scope) => _inner.Scoped(scope);
        IBinding IBinding.Using(IResolver resolver) => _inner.Using(resolver);
        IBinding IBinding.UsingRecursively(IResolver resolver) => _inner.UsingRecursively(resolver);
        IBinding IBinding.Named(string name) => _inner.Named(name);
        public IBinding WithDecoration(Func<IResolver, object, object> decoration) => _inner.WithDecoration(decoration);

        #endregion

        #region IBinding<T> members

        public IBinding<T> InList() => _inner.InList()
                                             .Typed<T>();

        public IBinding<T> AsSingle() => _inner.AsSingle()
                                               .Typed<T>();

        public IBinding<T> AsEagerSingle() => _inner.AsEagerSingle()
                                                    .Typed<T>();

        public IBinding<T> Id(BindingId id) => _inner.Id(id)
                                                     .Typed<T>();

        public IBinding<T> Alias(Type aliasAbstractionType) => _inner.Alias(aliasAbstractionType)
                                                                     .Typed<T>();

        public IBinding<T> Scoped(IScope scope) => _inner.Scoped(scope)
                                                         .Typed<T>();

        public IBinding<T> Using(IResolver resolver) => _inner.Using(resolver)
                                                              .Typed<T>();

        public IBinding<T> UsingRecursively(IResolver resolver) =>
            new TypedBinding<T>(_inner.UsingRecursively(resolver));

        public IBinding<T> Named(string name) =>
            new TypedBinding<T>(_inner.Named(name));

        public IBinding<T> WithDecoration(Func<IResolver, T, T> decoration) =>
            _inner.WithDecoration((resolver, inner) => decoration(resolver, (T) inner))
                  .Typed<T>();

        #endregion
    }
}