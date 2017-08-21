using System;

namespace Silphid.Injexit
{
    public class UnresolvedDependencyException : Exception
    {
        public Type DependentType { get; }
        public Type Type { get; }
        public string Id { get; }

        public UnresolvedDependencyException(Type dependentType, UnresolvedTypeException exception) :
            this(dependentType, exception.Type, exception.Id)
        {
        }
        
        public UnresolvedDependencyException(Type dependentType, Type type, string id)
        {
            DependentType = dependentType;
            Type = type;
            Id = id;
        }

        public override string Message
        {
            get
            {
                var withId = Id != null ? $" with Id {Id}" : "";
                return $"Failed to resolve dependency {Type.Name}{withId} of dependent type {DependentType.Name}.";
            }
        }
    }
}