using System;
using System.Text;
using log4net;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silphid.Loadzup.Resource
{
    public class ResourceLoader : LoaderBase
    {
        private readonly IConverter _converter;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResourceLoader));

        public ResourceLoader(IConverter converter)
        {
            _converter = converter;
        }

        public override bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.Resource;

        public override IObservable<T> Load<T>(Uri uri, Options options = null)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            var contentType = options?.ContentType;
            var path = GetPathAndContentType(uri, ref contentType, false);

            return LoadAsync<T>(path)
                .ContinueWith(x => Convert<T>(x, contentType))
                .DoOnError(ex => Log.Error($"Failed to load resource at '{path}' to type {typeof(T)}.", ex));
        }

        private bool IsUnityObject<T>() => typeof(T).IsAssignableTo<Object>();

        private IObservable<Object> LoadAsync<T>(string path)
        {
            return Observable.Defer(() =>
                Resources
                    .LoadAsync(path, IsUnityObject<T>() ? typeof(T) : typeof(Object))
                    .AsObservable<Object>());
        }

        private IObservable<T> Convert<T>(Object obj, ContentType contentType)
        {
            if (obj is T)
                return Observable.Return((T) (object) obj);

            if (obj is TextAsset)
            {
                var textAsset = (TextAsset) obj;
                return _converter.Convert<T>(textAsset.bytes, contentType, Encoding.UTF8);
            }

            if (_converter.Supports<T>(obj, contentType))
                return _converter.Convert<T>(obj, contentType, Encoding.UTF8);

            throw new NotSupportedException($"Conversion not supported from {obj.GetType().Name} to {typeof(T).Name}.");
        }
    }
}