using System;
using System.Collections.Generic;

namespace Silphid.Loadzup
{
    /// <summary>
    /// Loader decorator that tries to transform input Uri and/or IOptions via its filters in order, and stops after
    /// first successful filter. Note that some filters might decide to apply their transformation while still returning
    /// false, in which case more than one filter may actually be applied. 
    /// </summary>
    public class FirstFilterLoader : ILoader
    {
        private readonly ILoader _innerLoader;
        private readonly List<IFilter> _filters;

        public FirstFilterLoader(ILoader innerLoader, List<IFilter> filters)
        {
            _innerLoader = innerLoader;
            _filters = filters;
        }

        public bool Supports<T>(Uri uri) => _innerLoader.Supports<T>(uri);

        public IObservable<T> Load<T>(Uri uri, IOptions options = null)
        {
            foreach (var filter in _filters)
                if (filter.Apply(ref uri, ref options))
                    break;

            return _innerLoader.Load<T>(uri, options);
        }
    }
}