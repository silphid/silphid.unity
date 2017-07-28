using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Sequencit
{
    public static class ISequencerExtensions
    {
        public static void Add<T>(this ISequencer This, IObservable<T> observable)
        {
            This.Add(observable.AsSingleUnitObservable());
        }

        public static void Add<T>(this ISequencer This, Func<IObservable<T>> observableFactory)
        {
            This.Add(Observable.Defer(observableFactory));
        }

        public static void AddParallel(this ISequencer This, Action<Parallel> action)
        {
            This.Add(() => Parallel.Create(action));
        }

        public static void AddParallel(this ISequencer This, params Func<IObservable<Unit>>[] selectors)
        {
            This.Add(() => Parallel.Create(selectors));
        }

        public static void AddParallel(this ISequencer This, IEnumerable<IObservable<Unit>> observables)
        {
            This.Add(() => Parallel.Create(observables));
        }

        public static void AddSequence(this ISequencer This, Action<Sequence> action)
        {
            This.Add(() => Sequence.Create(action));
        }

        public static void AddSequence(this ISequencer This, params Func<IObservable<Unit>>[] selectors)
        {
            This.Add(() => Sequence.Create(selectors));
        }

        public static void AddSequence(this ISequencer This, IEnumerable<IObservable<Unit>> observables)
        {
            This.Add(() => Sequence.Create(observables));
        }

        public static void AddAction(this ISequencer This, Action action)
        {
            This.Add(() =>
            {
                action();
                return Observable.ReturnUnit();
            });
        }

        // Adds a gate that pauses sequencing until a given disposable is disposed.
        // It returns that disposable immediately, so that you store it and dispose
        // it at any point in time. You can even dispose it before the gate is
        // reached in the sequence.
        public static IDisposable AddDisposableGate(this ISequencer This)
        {
            var gate = new DisposableGate();
            This.Add(gate);
            return gate;
        }

        // Adds a gate that pauses sequencing until a given disposable is disposed.
        // It passes that disposable to a lambda expression that will be invoked
        // only when the gate is reached in the sequence, so that you may then
        // invoke some operation/animation/tween and finally dispose the disposable
        // once completed.
        public static void AddDisposableGate(this ISequencer This, Action<IDisposable> action)
        {
            This.Add(() => DisposableGate.Create(action));
        }

        // Adds a gate that pauses sequencing indefinitely when last emitted
        // value of an observable is false and resumes sequencing immediately
        // when it becomes true. It is recommended to use a BehaviorSubject or
        // a ReactiveProperty, because they always emit their current value
        // upon subscription.
        public static void AddGate(this ISequencer This, IObservable<bool> gate)
        {
            This.Add(() => gate.WhereTrue().Take(1));
        }

        public static void AddInterval(this ISequencer This, float seconds)
        {
            This.AddInterval(TimeSpan.FromSeconds(seconds));
        }

        public static void AddInterval(this ISequencer This, float seconds, IScheduler scheduler)
        {
            This.AddInterval(TimeSpan.FromSeconds(seconds), scheduler);
        }

        public static void AddInterval(this ISequencer This, TimeSpan interval)
        {
            This.AddInterval(interval, Scheduler.DefaultSchedulers.TimeBasedOperations);
        }

        public static void AddInterval(this ISequencer This, TimeSpan interval, IScheduler scheduler)
        {
            This.Add(Observable.ReturnUnit().Delay(interval, scheduler));
        }
    }
}