using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Sequencit
{
    public static class ISequencerExtensions
    {
        public static object Add<T>(this ISequencer This, IObservable<T> observable) =>
            This.Add(observable.AsSingleUnitObservable());

        public static object Add<T>(this ISequencer This, Func<IObservable<T>> observableFactory) =>
            This.Add(Observable.Defer(observableFactory));

        public static object AddParallel(this ISequencer This, Action<ISequencer> action) =>
            This.Add(() => Parallel.Create(action));

        public static object AddParallel(this ISequencer This, params Func<IObservable<Unit>>[] selectors) =>
            This.Add(() => Parallel.Create(selectors));

        public static object AddParallel(this ISequencer This, IEnumerable<IObservable<Unit>> observables) =>
            This.Add(() => Parallel.Create(observables));

        public static object AddSequence(this ISequencer This, Action<ISequencer> action) =>
            This.Add(() => Sequence.Create(action));

        public static object AddSequence(this ISequencer This, params Func<IObservable<Unit>>[] selectors) =>
            This.Add(() => Sequence.Create(selectors));

        public static object AddSequence(this ISequencer This, IEnumerable<IObservable<Unit>> observables) =>
            This.Add(() => Sequence.Create(observables));

        public static LiveSequence AddLiveSequence(this ISequencer This)
        {
            var liveSequence = new LiveSequence();
            This.Add(liveSequence);
            return liveSequence;
        }

        public static LiveSequence AddLiveSequence(this ISequencer This, Action<ISequencer> action)
        {
            var liveSequence = LiveSequence.Create(action);
            This.Add(liveSequence);
            return liveSequence;
        }

        public static object AddAction(this ISequencer This, Action action) =>
            This.Add(() =>
            {
                action();
                return new Instant();
            });

        // Adds an item that pauses sequencing until a given disposable is disposed.
        // It returns that disposable immediately, so that you store it and dispose
        // it at any point in time. You can even dispose it before the gate is
        // reached in the sequence.
        public static IDisposable AddLapse(this ISequencer This)
        {
            var lapse = new Lapse();
            This.Add(lapse);
            return lapse;
        }

        // Adds a gate that pauses sequencing until a given disposable is disposed.
        // It passes that disposable to a lambda expression that will be invoked
        // only when the gate is reached in the sequence, so that you may then
        // invoke some operation/animation/tween and finally dispose the disposable
        // once completed.
        public static object AddLapse(this ISequencer This, Action<IDisposable> action) =>
            This.Add(() => new Lapse(action));

        // Adds a gate that pauses sequencing indefinitely when last emitted
        // value of an observable is false and resumes sequencing immediately
        // when it becomes true. It is recommended to use a BehaviorSubject or
        // a ReactiveProperty, because they always emit their current value
        // upon subscription.
        public static object AddGate(this ISequencer This, IObservable<bool> gate) =>
            This.Add(() => gate.WhereTrue().Take(1));

        public static object AddDelay(this ISequencer This, float seconds) =>
            This.AddDelay(TimeSpan.FromSeconds(seconds));

        public static object AddDelay(this ISequencer This, float seconds, IScheduler scheduler) =>
            This.AddDelay(TimeSpan.FromSeconds(seconds), scheduler);

        public static object AddDelay(this ISequencer This, TimeSpan interval) =>
            This.AddDelay(interval, Scheduler.DefaultSchedulers.TimeBasedOperations);

        public static object AddDelay(this ISequencer This, TimeSpan interval, IScheduler scheduler) =>
            This.Add(Observable.Timer(interval, scheduler));

        public static object AddInstant(this ISequencer This)
        {
            var instant = new Instant();
            This.Add(instant);
            return instant;
        }
        
        public static object AddDispose(this ISequencer This, IDisposable disposable) =>
            This.AddAction(disposable.Dispose);
    }
}