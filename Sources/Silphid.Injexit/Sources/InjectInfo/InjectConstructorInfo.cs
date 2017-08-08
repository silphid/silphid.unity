using System;
using System.Reflection;

namespace Silphid.Injexit
{
    public class InjectConstructorInfo
    {
        public ConstructorInfo Constructor { get; }
        public Exception ConstructorException { get; }
        public InjectParameterInfo[] Parameters { get; }

        public InjectConstructorInfo(ConstructorInfo constructor, Exception constructorException, InjectParameterInfo[] parameters)
        {
            Constructor = constructor;
            ConstructorException = constructorException;
            Parameters = parameters;
        }
    }
}