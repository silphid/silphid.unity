using System;

namespace Silphid.Injexit
{
    public struct Result
    {
        public readonly Func<IResolver, object> Factory;
        public readonly Exception Exception;
        public readonly IScope Scope;

        public Result(Func<IResolver, object> factory, IScope scope)
            : this()
        {
            Factory = factory;
            Scope = scope;
        }

        public Result(Exception exception)
            : this()
        {
            Exception = exception;
        }
    }
}