using System;
using Silphid.Loadzup.Http;
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

        public override IObservable<T> Load<T>(Uri uri, IOptions options = null)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            var mediaType = options.GetMediaType();
            GetPathAndContentType(uri, ref mediaType, true);

            return LoadFile(uri, options, mediaType)
                  .ContinueWith(x => _converter.Convert<T>(x.Bytes, mediaType, x.Encoding))
                  .Catch<T, Exception>(ex => Observable.Throw<T>(new LoadException(ex, uri, options)));
        }

        private IObservable<Response> LoadFile(Uri uri, IOptions options, string mediaType) =>
            _requester.Request(uri, options.WithMediaType(mediaType));
    }
}