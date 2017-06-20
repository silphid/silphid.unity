using System;
using System.Collections.Generic;
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

        public static void AddSuspension(this ISequencer This, Action<IDisposable> action)
        {
            This.Add(() => Suspension.Create(action));
        }

        public static IDisposable AddSuspension(this ISequencer This)
        {
            var complete = new Suspension();
            This.Add(complete);
            return complete;
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