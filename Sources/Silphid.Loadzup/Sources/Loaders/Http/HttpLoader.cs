using System;
using UniRx;

namespace Silphid.Loadzup.Http
{
    public class HttpLoader : ILoader
    {
        private readonly IRequester _requester;
        private readonly IConverter _converter;

        public HttpLoader(IRequester requester, IConverter converter)
        {
            _requester = requester;
            _converter = converter;
        }

        public bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.Http || uri.Scheme == Scheme.Https;

        public IObservable<T> Load<T>(Uri uri, Options options = null) =>
            _requester
                .Request(uri, options)
                .ContinueWith(x => _converter.Convert<T>(x.Bytes, options?.ContentType ?? x.ContentType, x.Encoding));
    }
}