using System;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.StreamingFile
{
    public class StreamingFileLoader : ILoader
    {
        private const string _pathSeparator = "/";
        private readonly IHttpRequester _requester;
        private readonly IConverter _converter;

        public StreamingFileLoader(IHttpRequester requester, IConverter converter)
        {
            _requester = requester;
            _converter = converter;
        }

        public bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.StreamingFile;

        public IObservable<T> Load<T>(Uri uri, Options options = null)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            var contentType = options?.ContentType;
            uri.GetPathAndContentType(ref contentType, _pathSeparator, true);           

            return LoadFile(uri, options, contentType)
                .ContinueWith(
                    x =>
                        _converter.Convert<T>(x.Bytes, options?.ContentType ?? x.ContentType ?? contentType,
                            x.Encoding));
        }

        private IObservable<Response> LoadFile(Uri uri, Options options, ContentType contentType)
        {
            if (options == null)
                options = new Options();

            options.ContentType = contentType;
            return _requester.Request(uri, options);
        }
    }
}