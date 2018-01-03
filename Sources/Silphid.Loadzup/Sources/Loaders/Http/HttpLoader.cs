using System;
using UniRx;

namespace Silphid.Loadzup.Http
{
    public class HttpLoader : LoaderBase
    {    
        private readonly IHttpRequester _requester;
        private readonly IConverter _converter;

        public HttpLoader(IHttpRequester requester, IConverter converter)
        {
            _requester = requester;
            _converter = converter;
        }

        public override bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.Http || uri.Scheme == Scheme.Https;

        public override IObservable<T> Load<T>(Uri uri, Options options = null)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            return _requester
                .Request(uri, options)
                .ContinueWith(x => _converter
                    .Convert<T>(x.Bytes, options?.ContentType ?? x.ContentType, x.Encoding)
                    .Catch<T, Exception>(ex => Observable
                        .Throw<T>(new LoadException("Load failed", ex, uri, options))));
        }
    }
}