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
        /// Marks binding with given Alias, to allow reuse of binding in
        /// different contexts.
        /// </summary>
        IBinding Alias(string alias);
        
        /// <summary>
        /// Attaches a child resolver to this binding, which will be used
        /// to resolve its dependencies. Useful for compositing objects more
        /// explicitly.
        /// </summary>
        IBinding Using(IResolver resolver);
        
        /// <summary>
        /// Marks binding with given Named, when multiple bindings have same
        /// abstraction type and you want to explicitly specify which to use
        /// for which members using [Inject(Named="SomeId")] attributes.
        /// </summary>
        IBinding Named(string name);
    }
}