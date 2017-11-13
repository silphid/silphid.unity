using System;
using System.Collections.Generic;
using System.Net;
using Silphid.Extensions;
using UnityEngine.Networking;

namespace Silphid.Loadzup
{
    public class HttpException : LoadzupException
    {
        public Uri Uri { get; }
        public HttpStatusCode StatusCode { get; }
        public string ResponseBody { get; }
        public Dictionary<string, string> ResponseHeaders { get; }

        public HttpException(UnityWebRequest request) : this(new Uri(request.url), request.error)
        {
            StatusCode = (HttpStatusCode) request.responseCode;
            ResponseBody = request.downloadHandler.text;
            ResponseHeaders = request.GetResponseHeaders();
        }

        public HttpException(Uri uri, HttpStatusCode statusCode) : this(uri, $"StatusCode: {statusCode}")
        {
            ResponseBody = ((int) statusCode).ToString();
            StatusCode = statusCode;
        }

        public HttpException(Uri uri, string message = null, Exception innerException = null)
            : base(message, innerException)
        {
            Uri = uri;
        }

        public override string Message =>
            $"{base.Message}\r\n" +
            $"Uri: {Uri}\r\n" +
            $"ResponseBody:\r\n{ResponseBody}\r\n" +
            $"ResponseHeaders:\r\n{{\r\n{ResponseHeaders?.JoinAsString("  ")}}}";
    }
}