using System;

namespace Silphid.Loadzup
{
    public class LoadzupException : Exception
    {
        public LoadzupException()
        {
        }

        public LoadzupException(string message) : base(message)
        {
        }

        public LoadzupException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}