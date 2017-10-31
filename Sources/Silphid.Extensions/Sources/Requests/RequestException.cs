using System;

namespace Silphid.Requests
{
    public class RequestException : Exception, IRequest
    {
        public RequestException(Exception innerException) : this("Exception wrapped as request", innerException)
        {
        }

        public RequestException(string message = null, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}