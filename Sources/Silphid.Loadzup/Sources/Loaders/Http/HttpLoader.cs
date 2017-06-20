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

        public bool Supports(Uri uri) =>
            uri.Scheme == "http" || uri.Scheme == "https";

        public IObservable<T> Load<T>(Uri uri, Options options = null) =>
            _requester
                .Request(uri, options)
                .Select(x => _converter.Convert<T>(x.Bytes, options?.ContentType ?? x.ContentType, x.Encoding));
    }
}