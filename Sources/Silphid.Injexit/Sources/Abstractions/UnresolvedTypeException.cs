using System;

namespace Silphid.Injexit
{
    public class UnresolvedTypeException : Exception
    {
        public Type Type { get; }
        public string Id { get; }

        public UnresolvedTypeException(Type type, string id)
        {
            Type = type;
            Id = id;
        }

        public override string Message
        {
            get
            {
                var withId = Id != null ? $" with Id {Id}" : "";
                return $"Failed to resolve type {Type.Name} {withId}.";
            }
        }
    }
}