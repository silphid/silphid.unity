using System;

namespace Silphid.Injexit
{
    public interface IBinding
    {
        /// <summary>
        /// The container to which this binding belongs.
        /// </summary>
        IContainer Container { get; }

        /// <summary>
        /// Marks binding as being part of a list of bindings of the same
        /// abstraction type T, to be injected as some IEnumerable&lt;T&gt;,
        /// List&lt;T&gt; or T[].
        /// </summary>
        IBinding InList();

        /// <summary>
        /// Marks binding as singleton and for lazy loading. Multiple injections of the same
        /// abstraction type T will share the same instance.
        /// </summary>
        IBinding AsSingle();

        /// <summary>
        /// Marks binding as singleton and for eager loading (as opposed to the default, lazy loading).
        /// All such bindings can be forced to be instantiated by calling the container's
        /// InstantiateEagerSingles() method. This is particularly useful when no classes depend on them,
        /// but you want them to be instantiated anyway.
        /// </summary>
        IBinding AsEagerSingle();

        /// <summary>
        /// Marks binding with given BindingId object, to allow reuse of binding in different contexts with
        /// BindReference() and AddReference() (in lists).
        /// </summary>
        IBinding Id(BindingId id);

        /// <summary>
        /// Marks binding with a new BindingId object and returns that object, to allow reuse of binding in different
        /// contexts with BindReference() and AddReference() (in lists).
        /// </summary>
        BindingId Id();

        /// <summary>
        /// Marks binding with given Name, to allow reuse of binding in different contexts with BindReference()
        /// and AddReference() (in lists).
        /// </summary>
        IBinding Alias(Type aliasAbstractionType);

        /// <summary>
        /// Marks binding with given IScope, to allows instances to be cached in sub-container with that scope.
        /// </summary>
        IBinding Scoped(IScope scope);

        /// <summary>
        /// Attaches a child resolver to this binding, which will be used
        /// to resolve its direct dependencies. Useful for composing objects more
        /// explicitly.
        /// </summary>
        IBinding Using(IResolver resolver);

        /// <summary>
        /// Attaches a child resolver to this binding, which will be used
        /// to resolve its direct and indirect dependencies. Useful for composing objects more
        /// explicitly.
        /// </summary>
        IBinding UsingRecursively(IResolver resolver);

        /// <summary>
        /// Marks binding with given name, when multiple bindings have same
        /// abstraction type and you want to explicitly specify which to use
        /// for which members using [Inject(Named="SomeId")] attributes.
        /// </summary>
        IBinding Named(string name);

        /// <summary>
        /// Decorates instance upon creation.
        /// </summary>
        IBinding WithDecoration(Func<IResolver, object, object> decoration);
    }

    public interface IBinding<T> : IBinding
    {
        /// <summary>
        /// Marks binding as being part of a list of bindings of the same
        /// abstraction type T, to be injected as some IEnumerable&lt;T&gt;,
        /// List&lt;T&gt; or T[].
        /// </summary>
        new IBinding<T> InList();

        /// <summary>
        /// Marks binding as singleton and for lazy loading. Multiple injections of the same
        /// abstraction type T will share the same instance.
        /// </summary>
        new IBinding<T> AsSingle();

        /// <summary>
        /// Marks binding as singleton and for eager loading (as opposed to the default, lazy loading).
        /// All such bindings can be forced to be instantiated by calling the container's
        /// InstantiateEagerSingles() method. This is particularly useful when no classes depend on them,
        /// but you want them to be instantiated anyway.
        /// </summary>
        new IBinding<T> AsEagerSingle();

        /// <summary>
        /// Marks binding with given BindingId object, to allow reuse of binding in different contexts with
        /// BindReference() and AddReference() (in lists).
        /// </summary>
        new IBinding<T> Id(BindingId id);

        /// <summary>
        /// </summary>
        new IBinding<T> Alias(Type aliasAbstractionType);

        /// <summary>
        /// Marks binding with given IScope, to allows instances to be cached in sub-container with that scope.
        /// </summary>
        new IBinding<T> Scoped(IScope scope);

        /// <summary>
        /// Attaches a child resolver to this binding, which will be used
        /// to resolve its direct dependencies. Useful for compositing objects more
        /// explicitly.
        /// </summary>
        new IBinding<T> Using(IResolver resolver);

        /// <summary>
        /// Attaches a child resolver to this binding, which will be used
        /// to resolve its direct and indirect dependencies. Useful for compositing objects more
        /// explicitly.
        /// </summary>
        new IBinding<T> UsingRecursively(IResolver resolver);

        /// <summary>
        /// Marks binding with given Named, when multiple bindings have same
        /// abstraction type and you want to explicitly specify which to use
        /// for which members using [Inject(Named="SomeId")] attributes.
        /// </summary>
        new IBinding<T> Named(string name);

        /// <summary>
        /// Decorates instance upon creation.
        /// </summary>
        IBinding<T> WithDecoration(Func<IResolver, T, T> decoration);
    }
}