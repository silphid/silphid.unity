using System;

namespace Silphid.Showzup
{
    public class TransitionParentDestroyedException : Exception
    {
        public TransitionParentDestroyedException() {}

        public TransitionParentDestroyedException(string message)
            : base(message) {}

        public TransitionParentDestroyedException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}