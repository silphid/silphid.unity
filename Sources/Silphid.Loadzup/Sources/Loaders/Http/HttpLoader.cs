using System;
using log4net;
using UniRx;

namespace Silphid.Loadzup.Http
{
    public class HttpLoader : ILoader
    {    
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpLoader));
        
        private readonly IHttpRequester _requester;
        private readonly IConverter _converter;

        public HttpLoader(IHttpRequester requester, IConverter converter)
        {
            _requester = requester;
            _converter = converter;
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
                    .DoOnError(ex => Log.Error($"Failed to convert response from Uri {uri} and type {options?.ContentType ?? x.ContentType} to type {typeof(T)} : {x.Encoding.GetString(x.Bytes)}", ex)));
        }
    }
}