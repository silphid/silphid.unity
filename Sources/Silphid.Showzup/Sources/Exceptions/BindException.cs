using System;

namespace Silphid.Showzup
{
    public class BindException : Exception
    {
        public BindException()
        {
        }

        public BindException(string message) : base(message)
        {
        }

        public BindException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}