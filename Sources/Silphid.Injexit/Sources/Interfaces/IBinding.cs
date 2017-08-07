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
        IBinding AsList();
        
        /// <summary>
        /// Marks binding as singleton. Multiple injections of the same
        /// abstraction type T will share the same instance.
        /// </summary>
        IBinding AsSingle();
        
        /// <summary>
        /// Attaches a child resolver to this binding, which will be used
        /// to resolve its dependencies. Useful for compositing objects more
        /// explicitly.
        /// </summary>
        IBinding Using(IResolver resolver);
        
        /// <summary>
        /// Marks binding with given Id, when multiple bindings have same
        /// abstraction type and you want to explicitly specify which to use
        /// for which members using [Inject(Id="SomeId")] attributes.
        /// </summary>
        IBinding WithId(string id);
    }
}