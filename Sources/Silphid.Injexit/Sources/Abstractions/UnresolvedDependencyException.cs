using System;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public class UnresolvedDependencyException : Exception
    {
        public Type[] AncestorTypes { get; }
        public Type Type { get; }
        public string Id { get; }

        public UnresolvedDependencyException(Type parentType, UnresolvedTypeException exception) :
            this(parentType, exception.Type, exception.Id)
        {
        }

        public UnresolvedDependencyException(Type parentType, UnresolvedDependencyException exception) :
            this(exception.AncestorTypes.Prepend(parentType).ToArray(), exception.Type, exception.Id)
        {
        }
        
        public UnresolvedDependencyException(Type parentType, Type type, string id) :
            this(new []{ parentType }, type, id)
        {
        }
        
        public UnresolvedDependencyException(Type[] ancestorTypes, Type type, string id)
        {
            AncestorTypes = ancestorTypes;
            Type = type;
            Id = id;
        }

        public override string Message
        {
            get
            {
                var withId = Id != null ? $" with Id {Id}" : "";
                var ancestors = AncestorTypes.Select(x => x.Name).ToDelimitedString(" -> ");
                return $"Failed to resolve dependency {Type.Name}{withId} for chain: {ancestors}";
            }
        }
    }
}