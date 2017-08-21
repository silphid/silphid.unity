using System;

namespace Silphid.Injexit
{
    public class UnresolvedTypeException : Exception
    {
        public Type DependencyType { get; }
        public string Id { get; }

        public UnresolvedTypeException(Type dependencyType, string id)
        {
            DependencyType = dependencyType;
            Id = id;
        }

        public override string Message
        {
            get
            {
                var withId = Id != null ? $" with Id {Id}" : "";
                return $"Failed to resolve type {DependencyType.Name} {withId}.";
            }
        }
    }
}