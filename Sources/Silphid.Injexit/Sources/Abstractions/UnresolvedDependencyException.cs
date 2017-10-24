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
        public IResolver Resolver { get; }
        public string Reason { get; }

        public UnresolvedDependencyException(Type parentType, UnresolvedTypeException exception, string name) :
            this(new []{ parentType }, exception.Type, exception.Name, exception.Resolver, exception.Reason)
        {
        }

        public UnresolvedDependencyException(Type parentType, UnresolvedDependencyException exception, string name) :
            this(exception.AncestorTypes.Prepend(parentType).ToArray(), exception.Type, name, exception.Resolver, exception.Reason)
        {
        }
        
        public UnresolvedDependencyException(Type[] ancestorTypes, Type type, string name, IResolver resolver, string reason = null) : base(reason)
        {
            AncestorTypes = ancestorTypes;
            Type = type;
            Name = name;
            Resolver = resolver;
            Reason = reason;
        }

        public override string Message =>
            $"{base.Message}\r\n" +
            $"Abstraction: {Type.Name}\r\n" +
            $"Name: {Name}\r\n" +
            $"Dependent(s): {AncestorTypes.Select(x => x.Name).ConcatToString(" > ")}\r\n" +
            "Bindings:\r\n" +
            $"{Resolver}";
    }
}