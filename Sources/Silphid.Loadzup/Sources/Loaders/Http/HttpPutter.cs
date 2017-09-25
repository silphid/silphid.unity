using System;
using UniRx;

namespace Silphid.Loadzup.Http
{
    public class HttpPutter : IHttpPutter
    {
        private readonly IHttpRequester _requester;
        private readonly IConverter _converter;

        public HttpPutter(IHttpRequester requester, IConverter converter)
        {
            _requester = requester;
            _converter = converter;
        }

        public IObservable<T> Put<T>(Uri uri, string body, Options options = null) =>
            _requester
                .Put(uri, body, options)
                .ContinueWith(x => typeof(T) != typeof(Response)
                    ? _converter.Convert<T>(x.Bytes, options?.ContentType ?? x.ContentType, x.Encoding)
                    : Observable.Return((T) (object) x));
    }
}