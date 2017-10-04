using System;

namespace Silphid.Injexit
{
    public class UnresolvedTypeException : Exception
    {
        private readonly string _message;
            
        public Type Type { get; }
        public string Name { get; }

        public UnresolvedTypeException(Type type, string name, string message = null)
        {
            Type = type;
            Name = name;
            _message = message;
        }

        public override string Message
        {
            get
            {
                var named = Name != null ? $" for member named {Name}" : "";
                var message = _message != null ? $" : {_message}" : "";
                return $"Failed to resolve type {Type.Name}{named}{message}";
            }
        }
    }
}