using System;

namespace Silphid.Loadzup
{
    public class LoadException : LoadzupException
    {
        public Uri Uri { get; }
        public Options Options { get; }

        public LoadException(Uri uri, Options options)
        {
            Uri = uri;
            Options = options;
        }

        public LoadException(string message, Uri uri, Options options) : base(message)
        {
            Uri = uri;
            Options = options;
        }

        public LoadException(string message, Exception innerException, Uri uri, Options options) : base(message, innerException)
        {
            Uri = uri;
            Options = options;
        }

        public override string Message =>
            $"{base.Message}\r\n" +
            $"Uri: {Uri}\r\n";
    }
}