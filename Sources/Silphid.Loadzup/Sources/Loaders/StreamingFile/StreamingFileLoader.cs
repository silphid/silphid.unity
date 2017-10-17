using System;
using UniRx;

namespace Silphid.Loadzup.StreamingFile
{
    public class StreamingFileLoader : LoaderBase
    {
        private readonly IHttpRequester _requester;
        private readonly IConverter _converter;

        public StreamingFileLoader(IHttpRequester requester, IConverter converter)
        {
            _requester = requester;
            _converter = converter;
        }

        public override bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.StreamingFile;

        public override IObservable<T> Load<T>(Uri uri, Options options = null)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            var contentType = options?.ContentType;
            GetPathAndContentType(uri, ref contentType, true);           

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