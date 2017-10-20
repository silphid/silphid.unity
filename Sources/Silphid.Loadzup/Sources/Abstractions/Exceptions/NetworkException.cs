using System;

namespace Silphid.Loadzup
{
    public class NetworkException : LoadzupException
    {
        public NetworkException()
        {
        }

        public NetworkException(string message) : base(message)
        {
        }

        public NetworkException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}