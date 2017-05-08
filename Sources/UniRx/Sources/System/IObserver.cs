using System;

namespace UniRx
{
    public interface IObserver<T>
    {
        void OnCompleted();
        void OnError(Exception error);
        void OnNext(T value);
    }
}

#if !(NETFX_CORE || ENABLE_MONO_BLEEDING_EDGE_EDITOR || ENABLE_MONO_BLEEDING_EDGE_STANDALONE)

namespace System
{
    [Obsolete("Use UniRx version of this type to prevent conflicts with built-in type in Windows Store builds.", true)]
    public interface IObserver<T>
    {
        void OnCompleted();
        void OnError(Exception error);
        void OnNext(T value);
    }
}

#endif