using System;
using System.Text;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silphid.Loadzup.Resource
{
    public class ResourceLoader : LoaderBase
    {
        private readonly IConverter _converter;

        public ResourceLoader(IConverter converter)
        {
            _converter = converter;
        }

        public override bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.Resource;

        public override IObservable<T> Load<T>(Uri uri, IOptions options = null)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            var mediaType = options.GetMediaType();
            var path = GetPathAndContentType(uri, ref mediaType, false);

            return LoadAsync<T>(path)
                  .ContinueWith(x => Convert<T>(x, mediaType))
                  .Catch<T, Exception>(ex => Observable.Throw<T>(new LoadException(ex, uri, options)));
        }

        private bool IsUnityObject<T>() => typeof(T).IsAssignableTo<Object>();

        private IObservable<Object> LoadAsync<T>(string path)
        {
            return Observable.Defer(
                () => Resources.LoadAsync(
                                    path,
                                    IsUnityObject<T>()
                                        ? typeof(T)
                                        : typeof(Object))
                               .AsObservable<Object>());
        }

        private IObservable<T> Convert<T>(Object obj, string mediaType)
        {
            if (obj is T)
                return Observable.Return((T) (object) obj);

            if (obj is TextAsset)
            {
                var textAsset = (TextAsset) obj;
                return _converter.Convert<T>(textAsset.bytes, mediaType, Encoding.UTF8);
            }

            if (_converter.Supports<T>(obj, mediaType))
                return _converter.Convert<T>(obj, mediaType, Encoding.UTF8);

            throw new NotSupportedException($"Conversion not supported from {obj.GetType().Name} to {typeof(T).Name}.");
        }
    }
}