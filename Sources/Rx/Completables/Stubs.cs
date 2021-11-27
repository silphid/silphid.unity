using System;

namespace UniRx
{
    internal static class Stubs2
    {
        public static readonly Action Nop = () => {};
        public static readonly Action<Exception> Throw = ex => throw ex;

        // marker for CatchIgnore and Catch avoid iOS AOT problem.
        public static IObservable<TSource> CatchIgnore<TSource>(Exception ex)
        {
            return Observable.Empty<TSource>();
        }
    }

    internal static class Stubs2<T>
    {
        public static readonly Action<T> Ignore = (T t) => {};
        public static readonly Func<T, T> Identity = (T t) => t;
        public static readonly Action<Exception, T> Throw = (ex, _) => throw ex;
    }

    internal static class Stubs2<T1, T2>
    {
        public static readonly Action<T1, T2> Ignore = (x, y) => {};
        public static readonly Action<Exception, T1, T2> Throw = (ex, _, __) => throw ex;
    }

    internal static class Stubs2<T1, T2, T3>
    {
        public static readonly Action<T1, T2, T3> Ignore = (x, y, z) => {};
        public static readonly Action<Exception, T1, T2, T3> Throw = (ex, _, __, ___) => throw ex;
    }
}