using System;
using UniRx;
using Silphid.Extensions;

namespace Silphid.Machina
{
    public static class IMachineExtensions
    {
        public static IDisposable OnEnter<T>(this IMachine This, Action<T> action) =>
            This.Entering<T>()
                .Subscribe(action)
                .AddTo(This);

        public static IDisposable OnEnter<T>(this IMachine This, Action action) =>
            This.OnEnter<T>(_ => action());

        public static IDisposable OnEnterNot<T>(this IMachine This, Action<T> action) =>
            This.Entering<T>()
                .Subscribe(action)
                .AddTo(This);

        public static IDisposable OnEnterNot<T>(this IMachine This, Action action) =>
            This.OnEnterNot<T>(_ => action());

        public static IDisposable OnExit<T>(this IMachine This, Action<T> action) =>
            This.Exiting<T>()
                .Subscribe(action)
                .AddTo(This);

        public static IDisposable OnExit<T>(this IMachine This, Action action) =>
            This.OnExit<T>(_ => action());

        public static IObservable<StateTransition> Transitions(this IMachine This) =>
            This.State
                .PairWithPrevious()
                .Select(x => new StateTransition(x.Item1, x.Item2));

        public static IDisposable OnTransition<TSource, TTarget>(this IMachine This, Action<StateTransition<TSource, TTarget>> action) =>
            This.Transitioning<TSource, TTarget>()
                .Subscribe(action);

        public static IDisposable OnTransition<TSource, TTarget>(this IMachine This, Action action) =>
            This.OnTransition<TSource, TTarget>(_ => action());

        public static IObservable<T> Entering<T>(this IMachine This) =>
            This.State.OfType<object, T>();

        public static IObservable<object> EnteringNot<T>(this IMachine This) =>
            This.State.Where(x => !(x is T));

        public static IObservable<T> Exiting<T>(this IMachine This) =>
            This.Transitions()
                .Select(x => x.Source)
                .OfType<object, T>();

        public static IObservable<StateTransition<TSource, TTarget>> Transitioning<TSource, TTarget>(this IMachine This) =>
            This.Transitions()
                .Where(x => x.Source is TSource && x.Target is TTarget)
                .Select(x => new StateTransition<TSource, TTarget>((TSource) x.Source, (TTarget) x.Target));

        public static void Enter<T>(this IMachine This) where T : new() =>
            This.SetState(new T());

        public static bool IsState<T>(this IMachine This) =>
            This.State.Value is T;

        public static IObservable<bool> Is<T>(this IMachine This) =>
            This.State
                .Select(x => x is T)
                .DistinctUntilChanged();

        public static IObservable<bool> Is<T1, T2>(this IMachine This) =>
            This.State
                .Select(x => x is T1 || x is T2)
                .DistinctUntilChanged();

        public static IObservable<bool> Is<T1, T2, T3>(this IMachine This) =>
            This.State
                .Select(x => x is T1 || x is T2 || x is T3)
                .DistinctUntilChanged();

        public static IObservable<bool> IsNot<T>(this IMachine This) =>
            This.State
                .Select(x => !(x is T))
                .DistinctUntilChanged();

        public static IObservable<bool> Is<T>(this IMachine This, Func<T, bool> selector) =>
            This.State
                .Select(x => x is T state && selector(state))
                .DistinctUntilChanged();

        public static IObservable<long> EveryUpdateOf<T>(this IMachine This) =>
            This.Is<T>()
                .Select(x => x
                    ? Observable.EveryUpdate()
                    : Observable.Empty<long>())
                .Switch();
    }
}