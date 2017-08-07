using System.Collections.Generic;

namespace Silphid.Injexit
{
    public interface IInjector
    {
        /// <summary>
        /// Injects fields, properties and methods of given object,
        /// using given resolver, or itself if null/omitted.
        /// </summary>
        void Inject(object obj, IResolver resolver = null);

        /// <summary>
        /// Injects all given objects in two passes, firstly injecting
        /// fields and properties, secondly injecting methods. This allows
        /// methods marked with [Inject] attribute to serve as sort of
        /// constructors, as all fields and properties will already have
        /// been injected when those methods are called.
        /// </summary>
        void Inject(IEnumerable<object> objects, IResolver resolver = null);
    }
}