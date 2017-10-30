using System;

namespace Silphid.Extensions
{
    public static class ExceptionExtensions
    {
        public static T InnerExceptionOfType<T>(this Exception This) where T : Exception
        {
            var inner = This.InnerException;
            while (inner != null && !(inner is T))
                inner = inner.InnerException;

            return inner as T;
        }
    }
}