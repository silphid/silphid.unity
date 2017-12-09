using System.Linq;

namespace Silphid.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] RemoveInCopy<T>(this T[] This, T item)
        {
            return This.Except(item).ToArray();
        }

        public static T[] RemoveAtInCopy<T>(this T[] This, int index)
        {
            return This.Where((x, i) => i != index).ToArray();
        }

        public static T[] InsertInCopy<T>(this T[] This, int index, T item)
        {
            var list = This.ToList();
            list.Insert(index, item);
            return list.ToArray();
        }
    } 
}