using System;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public class UnresolvedDependencyException : Exception
    {
        public Type[] AncestorTypes { get; }
        public Type Type { get; }
        public string Name { get; } 

        public UnresolvedDependencyException(Type parentType, UnresolvedTypeException exception, string name) :
            this(parentType, exception.Type, exception.Name)
        {
        }

        public UnresolvedDependencyException(Type parentType, UnresolvedDependencyException exception, string name) :
            this(exception.AncestorTypes.Prepend(parentType).ToArray(), exception.Type, name)
        {
        }
        
        public UnresolvedDependencyException(Type parentType, Type type, string name) :
            this(new []{ parentType }, type, name)
        {
        }
        
        public UnresolvedDependencyException(Type[] ancestorTypes, Type type, string name)
        {
            AncestorTypes = ancestorTypes;
            Type = type;
            Name = name;
        }

        public override string Message
        {
            get
            {
                var ancestors = AncestorTypes.Select(x => x.Name).ToDelimitedString(" > ");
                return $"Failed to resolve dependency '{Name}' of type {Type.Name} for {ancestors}";
            }
        }
    }
}