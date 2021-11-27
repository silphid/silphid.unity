using System;
using UniRx.Completables.InternalUtil;

namespace UniRx
{
    public static class CompletableExtensions
    {
        public static IDisposable Subscribe(this ICompletable source)
        {
            return source.Subscribe(ThrowCompletableObserver.Instance);
        }

        public static IDisposable Subscribe(this ICompletable source, Action onCompleted)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeObserver(onCompleted, Completables.Stubs.Throw));
        }

        public static IDisposable Subscribe(this ICompletable source, Action<Exception> onError, Action onCompleted)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeObserver(onCompleted, onError));
        }

        public static IDisposable Subscribe(this ICompletable source, Action<Exception> onError)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeObserver(Completables.Stubs.Nop, onError));
        }

        public static IDisposable SubscribeWithState<TState>(this ICompletable source,
                                                             TState state,
                                                             Action<TState> onCompleted)
        {
            return source.Subscribe(
                CompletableObserver.CreateSubscribeWithStateObserver(state, Stubs2<TState>.Throw, onCompleted));
        }

        public static IDisposable SubscribeWithState<TState>(this ICompletable source,
                                                             TState state,
                                                             Action<Exception, TState> onError)
        {
            return source.Subscribe(
                CompletableObserver.CreateSubscribeWithStateObserver(state, onError, Stubs2<TState>.Ignore));
        }

        public static IDisposable SubscribeWithState<TState>(this ICompletable source,
                                                             TState state,
                                                             Action<Exception, TState> onError,
                                                             Action<TState> onCompleted)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeWithStateObserver(state, onError, onCompleted));
        }

        public static IDisposable SubscribeWithState2<TState1, TState2>(this ICompletable source,
                                                                        TState1 state1,
                                                                        TState2 state2,
                                                                        Action<TState1, TState2> onCompleted)
        {
            return source.Subscribe(
                CompletableObserver.CreateSubscribeWithState2Observer(
                    state1,
                    state2,
                    Stubs2<TState1, TState2>.Throw,
                    onCompleted));
        }

        public static IDisposable SubscribeWithState2<TState1, TState2>(this ICompletable source,
                                                                        TState1 state1,
                                                                        TState2 state2,
                                                                        Action<Exception, TState1, TState2> onError)
        {
            return source.Subscribe(
                CompletableObserver.CreateSubscribeWithState2Observer(
                    state1,
                    state2,
                    onError,
                    Stubs2<TState1, TState2>.Ignore));
        }

        public static IDisposable SubscribeWithState2<TState1, TState2>(this ICompletable source,
                                                                        TState1 state1,
                                                                        TState2 state2,
                                                                        Action<Exception, TState1, TState2> onError,
                                                                        Action<TState1, TState2> onCompleted)
        {
            return source.Subscribe(
                CompletableObserver.CreateSubscribeWithState2Observer(state1, state2, onError, onCompleted));
        }

        public static IDisposable SubscribeWithState3<TState1, TState2, TState3>(
            this ICompletable source,
            TState1 state1,
            TState2 state2,
            TState3 state3,
            Action<TState1, TState2, TState3> onCompleted)
        {
            return source.Subscribe(
                CompletableObserver.CreateSubscribeWithState3Observer(
                    state1,
                    state2,
                    state3,
                    Stubs2<TState1, TState2, TState3>.Throw,
                    onCompleted));
        }

        public static IDisposable SubscribeWithState3<TState1, TState2, TState3>(
            this ICompletable source,
            TState1 state1,
            TState2 state2,
            TState3 state3,
            Action<Exception, TState1, TState2, TState3> onError,
            Action<TState1, TState2, TState3> onCompleted)
        {
            return source.Subscribe(
                CompletableObserver.CreateSubscribeWithState3Observer(state1, state2, state3, onError, onCompleted));
        }
    }
}