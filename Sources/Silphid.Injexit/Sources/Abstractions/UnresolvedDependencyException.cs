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
        public string MemberName { get; } 

        public UnresolvedDependencyException(Type parentType, UnresolvedTypeException exception, string memberName) :
            this(parentType, exception.Type, exception.Id, memberName)
        {
        }

        public UnresolvedDependencyException(Type parentType, UnresolvedDependencyException exception, string memberName) :
            this(exception.AncestorTypes.Prepend(parentType).ToArray(), exception.Type, exception.Id, memberName)
        {
        }
        
        public UnresolvedDependencyException(Type parentType, Type type, string id, string memberName) :
            this(new []{ parentType }, type, id, memberName)
        {
        }
        
        public UnresolvedDependencyException(Type[] ancestorTypes, Type type, string id, string memberName)
        {
            AncestorTypes = ancestorTypes;
            Type = type;
            Id = id;
            MemberName = memberName;
        }

        public override string Message
        {
            get
            {
                var withId = Id != null ? $" with Id {Id}" : "";
                var ancestors = AncestorTypes.Select(x => x.Name).ToDelimitedString(" -> ");
                return $"Failed to resolve dependency '{MemberName}' of type {Type.Name}{withId} for chain: {ancestors}";
            }
        }
    }
}