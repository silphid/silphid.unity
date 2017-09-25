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

        public bool Supports<T>(object input, ContentType contentType) =>
            _children.Any(x => x.Supports<T>(input, contentType));

        public IObservable<T> Convert<T>(object input, ContentType contentType, Encoding encoding)
        {
            var child = _children.FirstOrDefault(x => x.Supports<T>(input, contentType));
            return child == null
                ? Observable.Throw<T>(new NotSupportedException($"Conversion not supported for content type {contentType} to {typeof(T).Name}."))
                : child.Convert<T>(input, contentType, encoding);
        }
    }
}