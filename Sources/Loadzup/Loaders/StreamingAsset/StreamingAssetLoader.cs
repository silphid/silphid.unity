using System;
using Silphid.Loadzup.Http;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.StreamingAsset
{
    public class StreamingAssetLoader : LoaderBase
    {
        private readonly IHttpRequester _requester;
        private readonly IConverter _converter;

        public StreamingAssetLoader(IHttpRequester requester, IConverter converter)
        {
            _requester = requester;
            _converter = converter;
        }

        public override bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.StreamingAsset;

        public override IObservable<T> Load<T>(Uri uri, IOptions options = null)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            var mediaType = options.GetMediaType();
            var path = GetPathAndContentType(uri, ref mediaType, true);

            return LoadFile(uri, options, path, mediaType)
                  .ContinueWith(x => _converter.Convert<T>(x.Bytes, mediaType, x.Encoding))
                  .Catch<T, Exception>(ex => Observable.Throw<T>(new LoadException(ex, uri, options)));
        }

        private IObservable<Response> LoadFile(Uri uri, IOptions options, string path, string mediaType)
        {
            var filePath = System.IO.Path.Combine(Application.streamingAssetsPath, path);
            return _requester.Request(new Uri(filePath), options.WithMediaType(mediaType));
        }
    }
}