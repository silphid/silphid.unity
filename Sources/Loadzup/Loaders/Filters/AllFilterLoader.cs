using System;
using System.Collections.Generic;

namespace Silphid.Loadzup
{
    /// <summary>
    /// Loader decorator that transforms input Uri and/or IOptions via all its filters in order, regardless of each
    /// filter's return value. Note that only filters that match will actually apply. 
    /// </summary>
    public class AllFilterLoader : ILoader
    {
        private readonly ILoader _innerLoader;
        private readonly List<IFilter> _filters;

        public AllFilterLoader(ILoader innerLoader, List<IFilter> filters)
        {
            _innerLoader = innerLoader;
            _filters = filters;
        }

        public bool Supports<T>(Uri uri) => _innerLoader.Supports<T>(uri);

        public IObservable<T> Load<T>(Uri uri, IOptions options = null)
        {
            foreach (var filter in _filters)
                filter.Apply(ref uri, ref options);

            return _innerLoader.Load<T>(uri, options);
        }
    }
}