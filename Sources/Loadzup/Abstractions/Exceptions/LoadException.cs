using System;

namespace Silphid.Loadzup
{
    public class LoadException : LoadzupException
    {
        public Uri Uri { get; }
        public IOptions Options { get; }

        public LoadException(Exception innerException, Uri uri, IOptions options = null)
            : base("Load failed", innerException)
        {
            Uri = uri;
            Options = options;
        }

        public LoadException(string message, Exception innerException, Uri uri, IOptions options = null)
            : base(message, innerException)
        {
            Uri = uri;
            Options = options;
        }

        public override string Message =>
            $"{base.Message}\r\n" + $"Uri: {Uri}\r\n" + $"Options: {Options}\n";
    }
}