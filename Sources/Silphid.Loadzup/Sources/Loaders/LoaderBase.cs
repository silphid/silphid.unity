using System;
using System.IO;
using Silphid.Extensions;

namespace Silphid.Loadzup
{
    public abstract class LoaderBase : ILoader
    {
        protected const string PathSeparator = "/";
        
        public abstract bool Supports<T>(Uri uri);
        public abstract IObservable<T> Load<T>(Uri uri, Options options = null);
        
        protected string GetPathAndContentType(Uri uri, ref ContentType contentType, bool keepExtension)
        {
            var path = GetPath(uri);

            // Any extension detected?
            var extension = Path.GetExtension(path);
            if (extension.IsNullOrWhiteSpace())
                return path;

            // Remove extension, because Unity doesn't expect it when looking up resources
            if (!keepExtension)
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

        protected string GetPath(Uri uri)
        {
            var host = uri.Host.RemovePrefix(PathSeparator);
            var isRoot = string.IsNullOrEmpty(uri.AbsolutePath) || uri.AbsolutePath == PathSeparator;
            return isRoot ? host : host + uri.PathAndQuery;
        }
    }
}