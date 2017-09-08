using System;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Http
{
    public class HttpLoader : ILoader
    {
        private readonly IRequester _requester;
        private readonly IConverter _converter;
        private readonly ILogger _logger;

        public HttpLoader(IRequester requester, IConverter converter, ILogger logger = null)
        {
            _requester = requester;
            _converter = converter;
            _logger = logger;
        }

        public bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.Http || uri.Scheme == Scheme.Https;

        public IObservable<T> Load<T>(Uri uri, Options options = null)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            return _requester
                .Request(uri, options)
                .ContinueWith(x => _converter
                    .Convert<T>(x.Bytes, options?.ContentType ?? x.ContentType, x.Encoding)
                    .DoOnError(ex => _logger?.LogError($"Failed to convert response from Uri {uri} to type {typeof(T)} : {x.Encoding.GetString(x.Bytes)}", ex)));
        }
    }
}