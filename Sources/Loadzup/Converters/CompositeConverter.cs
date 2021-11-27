using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;

namespace Silphid.Loadzup
{
    public class CompositeConverter : IConverter
    {
        private readonly List<IConverter> _children;

        public CompositeConverter(params IConverter[] children)
        {
            _children = children.ToList();
        }

        public bool Supports<T>(object input, string mediaType) =>
            _children.Any(x => x.Supports<T>(input, mediaType));

        public IObservable<T> Convert<T>(object input, string mediaType, Encoding encoding)
        {
            var child = _children.FirstOrDefault(x => x.Supports<T>(input, mediaType));
            return child == null
                       ? Observable.Throw<T>(
                           new NotSupportedException(
                               $"Conversion not supported for content type {mediaType} to {typeof(T).Name}."))
                       : child.Convert<T>(input, mediaType, encoding);
        }
    }
}