using System;
using System.Collections.Generic;
using System.Net;
using UniRx;

namespace Silphid.Loadzup
{
    public class RequestException : Exception
    {
        public string RawErrorMessage { get; private set; }
        public bool HasResponse { get; private set; }
        public string Text { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public Dictionary<string, string> ResponseHeaders { get; private set; }

        public RequestException()
        {
        }

        public RequestException(HttpStatusCode statusCode)
        {
            RawErrorMessage = statusCode.ToString();
            Text = ((int) statusCode).ToString();
            StatusCode = statusCode;
        }

        public RequestException(WWWErrorException exception)
        {
            RawErrorMessage = exception.RawErrorMessage;
            HasResponse = exception.HasResponse;
            Text = exception.Text;
            StatusCode = exception.StatusCode;
            ResponseHeaders = exception.ResponseHeaders;
        }

        public override string ToString() => $"{RawErrorMessage} {Text}";
    }
}