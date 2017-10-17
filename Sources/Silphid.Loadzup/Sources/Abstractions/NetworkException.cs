using System;

namespace Silphid.Loadzup
{
    public class NetworkException : Exception
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