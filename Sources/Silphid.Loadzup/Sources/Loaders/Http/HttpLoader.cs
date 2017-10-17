using System;
using log4net;
using UniRx;

namespace Silphid.Loadzup.Http
{
    public class HttpLoader : LoaderBase
    {    
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpLoader));
        
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
                    .DoOnError(ex => Log.Error($"Failed to convert response from Uri {uri} to type {typeof(T)} : {x.Encoding.GetString(x.Bytes)}", ex)));
        }
    }
}