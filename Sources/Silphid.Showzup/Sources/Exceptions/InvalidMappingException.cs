using System;

namespace Silphid.Showzup
{
    public class InvalidMappingException : Exception
    {
        private readonly Mapping _mapping;

        public InvalidMappingException(Mapping mapping, string message = null, Exception innerException = null)
            : base(message, innerException)
        {
            _mapping = mapping;
        }

        public override string Message => $"{base.Message} Mapping: {_mapping}";
    }
}