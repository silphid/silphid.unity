using System;

namespace Silphid.Injexit
{
    public class UnhandledScopeException : InjectionException
    {
        public UnhandledScopeException() {}

        public UnhandledScopeException(string scope, Exception innerException = null)
            : base(
                $"Unhandled scope {scope}. Make sure some intermediate container is marked with that scope.",
                innerException) {}
    }
}