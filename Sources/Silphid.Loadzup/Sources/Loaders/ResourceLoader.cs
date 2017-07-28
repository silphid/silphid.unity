using System;
using System.IO;
using System.Text;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silphid.Loadzup.Resource
{
    public class ResourceLoader : ILoader
    {
        private const string PathSeparator = "/";

        private readonly IConverter _converter;

        public ResourceLoader(IConverter converter)
        {
            _converter = converter;
        }

        public bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.Resource;

        public IObservable<T> Load<T>(Uri uri, Options options)
        {
            var contentType = options?.ContentType;
            var path = GetPathAndContentType(uri, ref contentType);

            return LoadAsync<T>(path)
                .ContinueWith(x => Convert<T>(x, contentType))
//                .DoOnCompleted(() => Debug.Log($"#Loadzup# Asset '{path}' loaded from resources."))
                .DoOnError(error => Debug.Log($"#Loadzup# Failed to load asset '{path}' from resources: {error}"));
        }

        private bool IsUnityObject<T>() => typeof(T).IsAssignableTo<Object>();

        private IObservable<Object> LoadAsync<T>(string path)
        {
            return Resources
                .LoadAsync(path, IsUnityObject<T>() ? typeof(T) : typeof(Object))
                .AsObservable<Object>();
        }

        private string GetPathAndContentType(Uri uri, ref ContentType contentType)
        {
            // Rebuild path while removing scheme component
            var host = uri.Host.RemovePrefix(PathSeparator);
            bool isRoot = string.IsNullOrEmpty(uri.AbsolutePath) || uri.AbsolutePath == PathSeparator;
            var path = isRoot ? host : host + uri.AbsolutePath;

            // Any extension detected?
            var extension = Path.GetExtension(path);
            if (extension.IsNullOrWhiteSpace())
                return path;

            // Remove extension, because Unity doesn't expect it when looking up resources
            path = path.Left(path.LastIndexOf(".", StringComparison.Ordinal));

            if (contentType == null)
            {
                // Try to determine content type from extension
                var mediaType = KnownMediaType.FromExtension(extension);
                if (mediaType != null)
                    contentType = new ContentType(mediaType);
            }

            return path;
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