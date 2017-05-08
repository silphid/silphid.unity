using System.Collections.Generic;

namespace Silphid.Extensions
{
    public static class ObjectExtensions
    {
        public static IEnumerable<T> ToSingleEnumerable<T>(this T instance)
        {
            return new[] { instance };
        }
    }
}