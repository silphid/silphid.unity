using System;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public class CircularDependencyException : InjectionException
    {
        public Type[] Types { get; }

        public CircularDependencyException(Type parentType, CircularDependencyException exception)
            : this(
                exception.Types.Prepend(parentType)
                         .ToArray()) {}

        public CircularDependencyException(Type[] types)
        {
            Types = types;
        }

        public CircularDependencyException(Type type)
        {
            Types = new[] { type };
        }

        public override string Message =>
            $"Circular dependency detected: {Types.Select(x => x.Name).JoinAsString(" > ")}";
    }
}