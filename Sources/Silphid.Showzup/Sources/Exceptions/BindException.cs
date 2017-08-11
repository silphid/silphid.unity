using System;

namespace Silphid.Showzup
{
    public class BindException: Exception
    {
        public BindException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}