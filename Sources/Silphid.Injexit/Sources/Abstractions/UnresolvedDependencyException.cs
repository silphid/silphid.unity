using System;

namespace Silphid.Injexit
{
    public class UnresolvedDependencyException : UnresolvedTypeException
    {
        public Type DependentType { get; }
        
        public UnresolvedDependencyException(Type dependentType, UnresolvedTypeException exception) :
            base(exception.DependencyType, exception.Id)
        {
            DependentType = dependentType;
        }
        
        public UnresolvedDependencyException(Type dependentType, Type dependencyType, string id) : base(dependencyType, id)
        {
            DependentType = dependentType;
        }
    }
}