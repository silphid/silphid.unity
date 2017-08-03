using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.StreamingAsset
{
    public class StreamingAssetLoader : ILoader
    {
        private readonly IRequester _requester;
        private readonly IConverter _converter;

        public StreamingAssetLoader(IRequester requester, IConverter converter)
        {
            _requester = requester;
            _converter = converter;
        }

        public bool Supports<T>(Uri uri) => 
            uri.Scheme == Scheme.StreamingAsset;

        public IObservable<T> Load<T>(Uri uri, Options options = null) =>
            LoadFile(uri, options)
                .ContinueWith(x => _converter.Convert<T>(x.Bytes, options?.ContentType ?? x.ContentType, x.Encoding));

        private IObservable<Response> LoadFile(Uri uri, Options options)
        {
            var filePath = System.IO.Path.Combine(Application.streamingAssetsPath, uri.Host);

            if (filePath.Contains("://"))
                return _requester.Request(uri, options);

            return Observable.Return(System.IO.File.ReadAllBytes(filePath))
                .Select(x => new Response(x, new Dictionary<string, string>()));
        }
    }
}