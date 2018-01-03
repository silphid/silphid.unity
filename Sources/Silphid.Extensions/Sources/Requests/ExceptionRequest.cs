using System;

namespace Silphid.Requests
{
    public class ExceptionRequest : Exception, IRequest
    {
        public ExceptionRequest(Exception innerException) : this("Exception wrapped as request", innerException)
        {
        }

        protected ExceptionRequest(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        protected ExceptionRequest() : base(null)
        {
        }
    }
}