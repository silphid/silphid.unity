using System;

namespace Silphid.Showzup
{
    public class InvalidManifestException : Exception
    {
        public InvalidManifestException() {}

        public InvalidManifestException(string message)
            : base(message) {}

        public InvalidManifestException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}