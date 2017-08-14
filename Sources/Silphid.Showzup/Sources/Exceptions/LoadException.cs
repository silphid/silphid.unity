using System;

namespace Silphid.Showzup
{
    public class LoadException : Exception
    {
        public LoadException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}