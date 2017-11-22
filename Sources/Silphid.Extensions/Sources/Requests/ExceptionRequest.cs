using System;

namespace Silphid.Requests
{
    public class ExceptionRequest : Exception, IRequest
    {
        public ExceptionRequest(Exception innerException) : this("Exception wrapped as request", innerException)
        {
        }

        public ExceptionRequest(string message = null, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}