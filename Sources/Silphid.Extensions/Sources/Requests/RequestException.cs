using System;

namespace Silphid.Requests
{
    public class RequestException : Exception, IRequest
    {
        protected RequestException()
        {
        }

        public RequestException(string message) : this(message, null)
        {
        }

        public RequestException(Exception innerException) : this("Wrapping exception into request", innerException)
        {
        }

        public RequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}