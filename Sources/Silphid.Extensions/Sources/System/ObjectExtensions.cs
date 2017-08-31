using System.Collections.Generic;

namespace Silphid.Extensions
{
    public static class ObjectExtensions
    {
        public static IEnumerable<T> ToSingleItemEnumerable<T>(this T This) =>
            new[] { This };

        public static List<T> ToSingleItemList<T>(this T This) =>
            new List<T>(new[] { This });
    }
}