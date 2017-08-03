using System;
using System.IO;
using Silphid.Extensions;

namespace Silphid.Loadzup
{
    public static class UriExtensions
    {
        public static string GetPathAndContentType(this Uri This, ref ContentType contentType,
            string pathSeparator, bool keepExtension)
        {
            // Rebuild path while removing scheme component
            var host = This.Host.RemovePrefix(pathSeparator);
            bool isRoot = string.IsNullOrEmpty(This.AbsolutePath) || This.AbsolutePath == pathSeparator;
            var path = isRoot ? host : host + This.AbsolutePath;

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
    }
}