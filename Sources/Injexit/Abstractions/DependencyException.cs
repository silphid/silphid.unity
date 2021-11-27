using System;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Injexit
{
    public static class UnresolvedDependencyExceptionExtensions
    {
        public static DependencyException With(this DependencyException This, IResolver resolver) =>
            new DependencyException(This.Type, This.DependentTypes, This.Name, This.Reason, resolver);

        public static DependencyException WithDependent(this DependencyException This, Type dependentType) =>
            new DependencyException(
                This.Type,
                This.DependentTypes.Prepend(dependentType)
                    .ToArray(),
                This.Name,
                This.Reason,
                This.Resolver);
    }

    public class DependencyException : InjectionException
    {
        public Type[] DependentTypes { get; }
        public Type Type { get; }
        public string Name { get; }
        public string Reason { get; }
        public IResolver Resolver { get; }

        public DependencyException(Type type, Type dependentType, string name, string reason, IResolver resolver)
            : this(
                type,
                dependentType != null
                    ? new[] { dependentType }
                    : new Type[] {},
                name,
                reason,
                resolver) {}

        internal DependencyException(Type type, Type[] dependentTypes, string name, string reason, IResolver resolver)
            : base(reason)
        {
            DependentTypes = dependentTypes;
            Type = type;
            Name = name;
            Reason = reason;
            Resolver = resolver;
        }

        public override string Message =>
            $"{base.Message}\r\n" + $"Abstraction: {Type.FullName}\r\n" + $"Name: {Name}\r\n" +
            $"Dependent(s): {DependentTypes.Select(x => x.FullName).JoinAsString(" > ")}\r\n" + "----------\r\n" +
            $"{Resolver}\r\n" + "----------";
    }
}