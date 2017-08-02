using System;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Http
{
    public class HttpLoader : ILoader, IPoster
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

        public IObservable<T> Post<T>(Uri uri, WWWForm form, Options options) =>
            _requester
                .Post(uri, form, options)
                .ContinueWith(x => typeof(T) != typeof(Response)
                    ? _converter.Convert<T>(x.Bytes, options?.ContentType ?? x.ContentType, x.Encoding)
                    : Observable.Return((T) (object) x));
    }
}