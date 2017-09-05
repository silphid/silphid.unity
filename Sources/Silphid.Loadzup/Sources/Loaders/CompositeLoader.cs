using System;
using System.Collections.Generic;
using System.Linq;

namespace Silphid.Loadzup
{
    public class CompositeLoader : ILoader
    {
        private readonly List<ILoader> _children;

        public CompositeLoader(params ILoader[] children)
        {
            _children = children.ToList();
        }

        public bool Supports<T>(Uri uri) =>
            _children.Any(x => x.Supports<T>(uri));

        public IObservable<T> Load<T>(Uri uri, Options options  = null)
        {
            var child = _children.FirstOrDefault(x => x.Supports<T>(uri));
            if (child == null)
                throw new NotSupportedException($"Uri not supported: {uri}");

            return child.Load<T>(uri, options);
        }
    }
}