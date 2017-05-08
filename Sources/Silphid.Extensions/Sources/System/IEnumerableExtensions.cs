using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silphid.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> This)
        {
            return This.Where(x => x != null);
        } 

        public static void DisposeAll<T>(this IEnumerable<T> This) where T : IDisposable
        {
            This.ForEach(x => x.Dispose());
        }

        public static bool AllTrue(this IEnumerable<bool> This)
        {
            return This.All(x => x);
        }

        public static bool AllFalse(this IEnumerable<bool> This)
        {
            return This.All(x => !x);
        }

        public static bool AnyTrue(this IEnumerable<bool> This)
        {
            return This.Any(x => x);
        }

        public static bool AnyFalse(this IEnumerable<bool> This)
        {
            return This.Any(x => !x);
        }

		public static IEnumerable<bool> WhereTrue(this IEnumerable<bool> This)
		{
			return This.Where(x => x);
		} 

		public static IEnumerable<bool> WhereFalse(this IEnumerable<bool> This)
		{
			return This.Where(x => !x);
		} 

        public static IEnumerable<IEnumerable<T>> Paged<T>(this IEnumerable<T> source, int itemsPerPage)
        {
            var list = source.ToList();
            int index = 0;
            while (index < list.Count)
            {
                int count = Math.Min(itemsPerPage, list.Count - index);
                var array = new T[count];
                list.CopyTo(index, array, 0, count);
                index += count;
                yield return array;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random random = new Random();
            for (int i = 0; i < list.Count; i++)
            {
                // Permutate with random item
                int j = random.Next(list.Count);
                T temp = list[j];
                list[j] = list[i];
                list[i] = temp;
            }
        }

        /// <summary>
        /// Returns element for which selector function returns the minimum value.
        /// </summary>
        public static T WithMin<T, U>(this IEnumerable<T> source, Func<T, IComparable<U>> selector)
        {
            T minElement = default(T);
            IComparable<U> minValue = default(IComparable<U>);

            foreach (T element in source)
            {
                IComparable<U> value = selector(element);
                if (Equals(minElement, default(T)) || value.CompareTo((U) minValue) < 0)
                {
                    minValue = value;
                    minElement = element;
                }
            }

            return minElement;
        }

        public static T WithMax<T, U>(this IEnumerable<T> source, Func<T, IComparable<U>> selector)
        {
            T maxElement = default(T);
            IComparable<U> maxValue = default(IComparable<U>);

            foreach (T element in source)
            {
                IComparable<U> value = selector(element);
                if (Equals(maxElement, default(T)) || value.CompareTo((U) maxValue) > 0)
                {
                    maxValue = value;
                    maxElement = element;
                }
            }

            return maxElement;
        }

        public static string ToDelimitedString<T>(this IEnumerable<T> source, string delimiter)
        {
            StringBuilder builder = new StringBuilder();
            foreach (T t in source)
            {
                if (builder.Length > 0)
                {
                    builder.Append(delimiter);
                }
                builder.Append(t);
            }
            return builder.ToString();
        }

        public static bool OrderedEquals<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            // Compare lengths (list-specific optimization)
            if (source is IList<T> &&
                other is IList<T>)
            {
                int sourceCount = ((IList<T>)source).Count;
                int otherCount = ((IList<T>)other).Count;
                if (sourceCount != otherCount)
                    return false;
            }

            // Compare items one-by-one
            IEnumerator<T> sourceEnumerator = source.GetEnumerator();
            IEnumerator<T> otherEnumerator = other.GetEnumerator();
            bool hasSource, hasOther;
            while ((hasSource = sourceEnumerator.MoveNext()) &
                   (hasOther = otherEnumerator.MoveNext()))
            {
                if (!Equals(sourceEnumerator.Current, otherEnumerator.Current))
                    return false;
            }

            // Any item left in either collection?
            if (hasSource || hasOther)
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T obj)
        {
            T[] array = {obj};
            return array.Concat(source);
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T obj)
        {
            T[] array = {obj};
            return source.Concat(array);
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T obj)
        {
            return source.Where(s => !s.Equals(obj));
        }

        public static List<T> Shuffled<T>(this IEnumerable<T> source)
        {
            List<T> list = source.ToList();
            list.Shuffle();
            return list;
        }

        public static int FirstIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            int index = 0;
            foreach (T item in source)
            {
                if (predicate(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static int LastIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            List<T> list = source.ToList();
            list.Reverse();
            int index = list.Count - 1;
            foreach (T item in list)
            {
                if (predicate(item))
                {
                    return index;
                }
                index--;
            }
            return -1;
        }

        /// <summary>
        /// Compares objects based on their value modulated through custom function.
        /// </summary>
        private class CustomFunctionComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, object> _customFunc;

            public CustomFunctionComparer(Func<T, object> customFunc)
            {
                _customFunc = customFunc;
            }

            public bool Equals(T x, T y)
            {
                return Equals(_customFunc.Invoke(x), _customFunc.Invoke(y));
            }

            public int GetHashCode(T obj)
            {
                return _customFunc.Invoke(obj).GetHashCode();
            }
        }

        /// <summary>
        /// Variant of the Distinct() extension method allowing to specify a
        /// function to determine distinctive criteria for each item.
        /// </summary>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, object> comparisonValueFunc)
        {
            return source.Distinct(new CustomFunctionComparer<T>(comparisonValueFunc));
        }

        /// <summary>
        /// Invokes action on each item of source.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        /// <summary>
        /// Invokes action on each item of source, also passing in the index of item within collection.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            int index = 0;
            foreach (var item in source)
            {
                action(index++, item);
            }
        }

        /// <summary>
        /// Invokes action on items of source as they are iterated through downstream.
        /// </summary>
        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
                yield return item;
            }
        }

        public static T SelectRandom<T>(this IEnumerable<T> source)
        {
            var random = new Random();
            var list = source as IList<T> ?? source.ToList();
            var index = random.Next(0, list.Count);

            if (index < list.Count)
                return list[index];

            return default(T);
        }

//
//        public class IndexedPair<T>
//        {
//            public int Index { get; set; }
//            public T Value { get; set; }
//        }
//
//        public static IEnumerable<IndexedPair<T>> Indexed<T>(this IEnumerable<T> source)
//        {
//            int index = 0;
//            foreach (var item in source)
//            {
//                yield return new IndexedPair<T>
//                    {
//                        Index = index++,
//                        Value = item
//                    };
//            }
//        }
    }
}