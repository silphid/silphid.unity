using System;

namespace Silphid.Showzup.Layout
{
    public class CircularBindingException : Exception
    {
        public CircularBindingException() {}

        public CircularBindingException(string message)
            : base(message) {}

        public CircularBindingException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}