using System;

namespace Silphid.Requests
{
    public class RequestException : Exception, IRequest
    {
        public RequestException()
        {
        }

        public RequestException(string message) : base(message)
        {
        }

        public RequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}