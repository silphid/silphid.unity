using System;

namespace UniRx
{
    public interface IObservable<T>
    {
        IDisposable Subscribe(IObserver<T> observer);
    }

    public interface IGroupedObservable<TKey, TElement> : IObservable<TElement>
    {
        TKey Key { get; }
    }
}

#if !(NETFX_CORE || ENABLE_MONO_BLEEDING_EDGE_EDITOR || ENABLE_MONO_BLEEDING_EDGE_STANDALONE)

namespace System
{
    [Obsolete("Use UniRx version of this type to prevent conflicts with built-in type in Windows Store builds.", true)]
    public interface IObservable<T>
    {
        IDisposable Subscribe(IObserver<T> observer);
    }
}

#endif