using System;

namespace Silphid.Showzup
{
    public class PhaseCancelledException : Exception
    {
        public PhaseCancelledException()
        {
        }

        public PhaseCancelledException(string message) : base(message)
        {
        }

        public PhaseCancelledException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}