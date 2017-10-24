using System;

namespace Silphid.Injexit
{
    public class UnresolvedTypeException : Exception
    {
        public Type Type { get; }
        public string Name { get; }
        public IResolver Resolver { get; }
        public string Reason { get; }

        public UnresolvedTypeException(Type type, string name, IResolver resolver, string reason) : base(reason)
        {
            Type = type;
            Name = name;
            Resolver = resolver;
            Reason = reason;
        }

        public override string Message =>
            $"{base.Message}\r\n" +
            $"Abstraction Type: {Type.Name}\r\n" +
            $"Dependency Name: {Name}\r\n" +
            "Bindings:\r\n" +
            $"{Resolver}";
    }
}