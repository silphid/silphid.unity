using System;
using System.Text;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silphid.Loadzup.Resource
{
    public class ResourceLoader : ILoader
    {
        private const string _pathSeparator = "/";

        private readonly IConverter _converter;
        private readonly ILogger _logger;

        public ResourceLoader(IConverter converter, ILogger logger = null)
        {
            _converter = converter;
            _logger = logger;
        }

        public bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.Resource;

        public IObservable<T> Load<T>(Uri uri, Options options)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            var contentType = options?.ContentType;
            var path = uri.GetPathAndContentType(ref contentType, _pathSeparator, false);

            return LoadAsync<T>(path)
                .ContinueWith(x => Convert<T>(x, contentType))
                .DoOnError(ex => _logger?.LogError($"#Loadzup# Failed to load resource at '{path}' to type {typeof(T)}.", ex));
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

            throw new NotSupportedException($"Conversion not supported from {obj.GetType().Name} to {typeof(T).Name}.");
        }
    }
}