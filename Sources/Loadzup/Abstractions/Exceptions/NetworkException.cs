using System;
using UnityEngine.Networking;

namespace Silphid.Loadzup
{
    public class NetworkException : LoadzupException
    {
        public UnityWebRequest WebRequest { get; }
        public Uri Uri { get; }

        public NetworkException(Uri uri, string message, Exception innerException = null)
            : base(message, innerException)
        {
            Uri = uri;
        }

        public NetworkException(UnityWebRequest webRequest)
            : this(new Uri(webRequest.url), webRequest.error)
        {
            WebRequest = webRequest;
        }

        public override string Message =>
            $"{base.Message}\r\n" + $"Uri: {Uri}";
    }
}