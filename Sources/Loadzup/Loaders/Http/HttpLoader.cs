using System;
using Silphid.Extensions;
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

        public override IObservable<T> Load<T>(Uri uri, IOptions options = null)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            options = AddTextureModeIfNeeded<T>(options);
            uri = WithQueryParams(uri, options);

            return _requester.Request(uri, options)
                             .ContinueWith(response => Convert<T>(uri, options, response));
        }

        private IObservable<T> Convert<T>(Uri uri, IOptions options, Response response)
        {
            var value = (object) response.Texture ?? response.Bytes;

            if (value == null)
                return Observable.Return(default(T));

            // Value already of requested type?
            if (value.GetType()
                     .IsAssignableTo<T>())
                return Observable.Return((T) value);

            // Convert to requested type
            var mediaType = options.GetMediaType() ?? response.Headers.ContentType?.MediaType;
            return _converter.Convert<T>(value, mediaType, response.Encoding)
                             .Catch<T, Exception>(ex => Observable.Throw<T>(new LoadException(ex, uri, options)));
        }

        private IOptions AddTextureModeIfNeeded<T>(IOptions options) =>
            typeof(T) == typeof(Texture2D)
                ? options.WithTextureMode()
                : options;

        private Uri WithQueryParams(Uri uri, IOptions options) =>
            uri.WithQueryParams(options.GetQueryParams());
    }
}