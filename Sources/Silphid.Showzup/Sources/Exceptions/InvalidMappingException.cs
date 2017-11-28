using System;

namespace Silphid.Showzup
{
    public class InvalidMappingException : Exception
    {
        private readonly Mapping _mapping;
        
        public InvalidMappingException(Mapping mapping)
        {
        }

        public InvalidMappingException(Mapping mapping, string message) : base(message)
        {
        }

        public InvalidMappingException(Mapping mapping, string message, Exception innerException) : base(message, innerException)
        {
        }

        public override string Message => $"{base.Message} Mapping: {_mapping}";
    }
}