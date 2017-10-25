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
        public string ErrorMessage { get; }
        public string ResponseBody { get; }
        public Dictionary<string, string> ResponseHeaders { get; }

        public HttpException(UnityWebRequest request)
        {
            Uri = new Uri(request.url);
            StatusCode = (HttpStatusCode) request.responseCode;
            ErrorMessage = request.error;
            ResponseBody = request.downloadHandler.text;
            ResponseHeaders = request.GetResponseHeaders();
        }

        public HttpException(Uri uri, HttpStatusCode statusCode)
        {
            Uri = uri;
            ErrorMessage = $"StatusCode: {statusCode}";
            ResponseBody = ((int) statusCode).ToString();
            StatusCode = statusCode;
        }

        public HttpException(Uri uri, string message)
        {
            Uri = uri;
            ErrorMessage = message;
        }

        public override string Message =>
            $"{ErrorMessage}\r\n" +
            $"Uri: {Uri}\r\n" +
            $"ResponseBody:\r\n{ResponseBody}\r\n" +
            $"ResponseHeaders:\r\n{{\r\n{ResponseHeaders.JoinAsString("  ")}}}";
    }
}