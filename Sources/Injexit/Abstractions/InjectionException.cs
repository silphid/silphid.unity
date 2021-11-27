using System;

namespace Silphid.Injexit
{
    public class InjectionException : Exception
    {
        public InjectionException() {}

        public InjectionException(string message)
            : base(message) {}

        public InjectionException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}