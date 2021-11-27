using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class IEnumerableExtensions
    {
        public static TTarget SelectFirstOrDefault<TSource, TTarget>(this IEnumerable<TSource> source,
                                                                     Func<TSource, bool> predicate,
                                                                     Func<TSource, TTarget> selector,
                                                                     TTarget defaultValue = default(TTarget))
        {
            foreach (var element in source)
                if (predicate(element))
                    return selector(element);

            return defaultValue;
        }

        public static TTarget SelectLastOrDefault<TSource, TTarget>(this IEnumerable<TSource> source,
                                                                    Func<TSource, bool> predicate,
                                                                    Func<TSource, TTarget> selector,
                                                                    TTarget defaultValue = default(TTarget))
        {
            var value = default(TSource);
            var isFound = false;

            foreach (var element in source)
                if (predicate(element))
                {
                    value = element;
                    isFound = true;
                }

            return isFound
                       ? selector(value)
                       : defaultValue;
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate, T defaultValue)
        {
            foreach (var element in source)
                if (predicate(element))
                    return element;

            return defaultValue;
        }

        public static T LastOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate, T defaultValue)
        {
            var value = defaultValue;

            foreach (var element in source)
                if (predicate(element))
                    value = element;

            return value;
        }

        public static T FirstNotNullOrDefault<T>(this IEnumerable<T> This) =>
            This.FirstOrDefault(x => x != null);

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> This) =>
            This.Where(x => x != null);

        public static void DisposeAll<T>(this IEnumerable<T> This) where T : IDisposable
        {
            This.ForEach(x => x.Dispose());
        }

        public static bool AllTrue(this IEnumerable<bool> This) =>
            This.All(x => x);

        public static bool AllFalse(this IEnumerable<bool> This) =>
            This.All(x => !x);

        public static bool AnyTrue(this IEnumerable<bool> This) =>
            This.Any(x => x);

        public static bool AnyFalse(this IEnumerable<bool> This) =>
            This.Any(x => !x);

        public static IEnumerable<bool> WhereTrue(this IEnumerable<bool> This) =>
            This.Where(x => x);

        public static IEnumerable<bool> WhereFalse(this IEnumerable<bool> This) =>
            This.Where(x => !x);

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
            var random = new Random();
            for (int i = 0; i < list.Count; i++)
            {
                // Permute with random item
                int j = random.Next(list.Count);
                var temp = list[j];
                list[j] = list[i];
                list[i] = temp;
            }
        }

        /// <summary>
        /// Returns element for which selector function returns the minimum value.
        /// </summary>
        public static T WithMin<T, U>(this IEnumerable<T> source, Func<T, IComparable<U>> selector)
        {
            var minElement = default(T);
            var minValue = default(IComparable<U>);

            foreach (var element in source)
            {
                var value = selector(element);
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
            var maxElement = default(T);
            var maxValue = default(IComparable<U>);

            foreach (var element in source)
            {
                var value = selector(element);
                if (Equals(maxElement, default(T)) || value.CompareTo((U) maxValue) > 0)
                {
                    maxValue = value;
                    maxElement = element;
                }
            }

            return maxElement;
        }

        public static string JoinAsString<T>(this IEnumerable<T> source, string delimiter = null)
        {
            if (source == null)
                return string.Empty;

            var builder = new StringBuilder();
            bool needsDelimiter = false;
            foreach (var t in source.WhereNotNull())
            {
                if (delimiter != null && needsDelimiter)
                    builder.Append(delimiter);

                builder.Append(t);
                needsDelimiter = true;
            }

            return builder.ToString();
        }

        public static string JoinAsString<T>(this IEnumerable<T> source, string prefix, string suffix, bool omitLastSuffix = false)
        {
            if (source == null)
                return string.Empty;

            var builder = new StringBuilder();
            bool isFirst = true;
            foreach (var t in source.WhereNotNull())
            {
                if (!isFirst)
                    builder.Append(suffix);
                
                builder.Append(prefix);
                builder.Append(t);

                isFirst = false;
            }
            
            if (!omitLastSuffix && !isFirst)
                builder.Append(suffix);

            return builder.ToString();
        }

        public static bool OrderedEquals<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            // Compare lengths (list-specific optimization)
            if (source is IList<T> && other is IList<T>)
            {
                int sourceCount = ((IList<T>) source).Count;
                int otherCount = ((IList<T>) other).Count;
                if (sourceCount != otherCount)
                    return false;
            }

            // Compare items one-by-one
            using (var sourceEnumerator = source.GetEnumerator())
            using (var otherEnumerator = other.GetEnumerator())
            {
                bool hasSource, hasOther;
                while ((hasSource = sourceEnumerator.MoveNext()) & (hasOther = otherEnumerator.MoveNext()))
                {
                    if (!Equals(sourceEnumerator.Current, otherEnumerator.Current))
                        return false;
                }

                // Any item left in either collection?
                if (hasSource || hasOther)
                {
                    return false;
                }
            }

            return true;
        }

#if !UNITY_2018_1_OR_NEWER
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
#endif

        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int count)
        {
            source = source as T[] ?? source as IList<T> ?? source.ToList();
            for (int i = 0; i < count; i++)
                foreach (var s in source)
                    yield return s;
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T obj)
        {
            return source.Where(s => !s.Equals(obj));
        }

        public static List<T> Shuffled<T>(this IEnumerable<T> source)
        {
            var list = source.ToList();
            list.Shuffle();
            return list;
        }

        public static int FirstIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            int index = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                    return index;
                index++;
            }

            return -1;
        }

        public static int FirstIndex<T>(this IReadOnlyList<T> source, int startIndex, Predicate<T> predicate)
        {
            for (int i = startIndex; i < source.Count; i++)
                if (predicate(source[i]))
                    return i;

            return -1;
        }

        public static int LastIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            var list = source.ToList();
            list.Reverse();
            int index = list.Count - 1;
            foreach (var item in list)
            {
                if (predicate(item))
                {
                    return index;
                }

                index--;
            }

            return -1;
        }

        public static int? IndexOf<T>(this IEnumerable<T> This, T item)
        {
            var index = 0;
            foreach (var candidate in This)
            {
                if (candidate.Equals(item))
                    return index;

                index++;
            }

            return null;
        }

        public static int? IndexOf<T>(this IEnumerable<T> This, Func<T, bool> predicate)
        {
            var index = 0;
            foreach (var candidate in This)
            {
                if (predicate(candidate))
                    return index;

                index++;
            }

            return null;
        }

        public static T GetAtOrDefault<T>(this IEnumerable<T> This, int? index, T defaultValue = default(T))
        {
            if (index == null || index < 0)
                return defaultValue;

            var list = This as IList<T>;
            if (list != null)
                return index < list.Count
                           ? list[index.Value]
                           : defaultValue;

            var i = 0;
            foreach (var item in This)
                if (i++ == index)
                    return item;

            return defaultValue;
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
                return _customFunc.Invoke(obj)
                                  .GetHashCode();
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

        [Pure]
        public static IEnumerable<TR> SelectNotNull<T, TR>(this IEnumerable<T> This, Func<T, TR> selector) =>
            This.Select(selector)
                .WhereNotNull();
    }
}