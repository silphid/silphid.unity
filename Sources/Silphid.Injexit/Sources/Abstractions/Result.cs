using System;

namespace Silphid.Injexit
{
    public struct Result
    {
        public readonly object Instance;
        public readonly Func<IResolver, object> Factory;
        public readonly Exception Exception;

        public static Result ForInstance(object instance) => new Result(instance);

        private Result(object instance) : this()
        {
            Instance = instance;
        }


        public Result(Func<IResolver, object> factory) : this()
        {
            Factory = factory;
        }

        public Result(Exception exception) : this()
        {
            Exception = exception;
        }
    }
}