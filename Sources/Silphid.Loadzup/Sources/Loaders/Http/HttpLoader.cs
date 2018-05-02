using System;
using System.Collections.Concurrent;
using UniRx;
using UnityEngine;

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

            if (typeof(T) == typeof(Texture2D))
            {
                if(options == null)
                    options = new Options();
                
                if(options.CustomValues == null)
                    options.CustomValues = new ConcurrentDictionary<object, object>();
                
                options.CustomValues.Add("TextureRequested", true);
            }

            return _requester
                .Request(uri, options)
                .ContinueWith(x => _converter
                    .Convert<T>(x.Texture ?? (object) x.Bytes, options?.ContentType ?? x.ContentType, x.Encoding)
                    .Catch<T, Exception>(ex => Observable
                        .Throw<T>(new LoadException("Load failed", ex, uri, options))));
        }
    }
}