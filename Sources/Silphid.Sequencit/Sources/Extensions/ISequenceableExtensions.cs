using System;
using System.Collections.Generic;
using UniRx;

namespace Silphid.Sequencit
{
    public static class ISequenceableExtensions
    {
        public static void Add<T>(this ISequenceable This, UniRx.IObservable<T> observable)
        {
            This.Add(observable.AsSingleUnitObservable());
        }

        public static void Add<T>(this ISequenceable This, Func<UniRx.IObservable<T>> observableFactory)
        {
            This.Add(Observable.Defer(observableFactory));
        }

        public static void AddParallel(this ISequenceable This, Action<Parallel> action)
        {
            This.Add(() => Parallel.Create(action));
        }

        public static void AddParallel(this ISequenceable This, params Func<UniRx.IObservable<Unit>>[] selectors)
        {
            This.Add(() => Parallel.Create(selectors));
        }

        public static void AddParallel(this ISequenceable This, IEnumerable<UniRx.IObservable<Unit>> observables)
        {
            This.Add(() => Parallel.Create(observables));
        }

        public static void AddSequence(this ISequenceable This, Action<Sequence> action)
        {
            This.Add(() => Sequence.Create(action));
        }

        public static void AddSequence(this ISequenceable This, params Func<UniRx.IObservable<Unit>>[] selectors)
        {
            This.Add(() => Sequence.Create(selectors));
        }

        public static void AddSequence(this ISequenceable This, IEnumerable<UniRx.IObservable<Unit>> observables)
        {
            This.Add(() => Sequence.Create(observables));
        }

        public static void AddAction(this ISequenceable This, Action action)
        {
            This.Add(() =>
            {
                action();
                return Observable.ReturnUnit();
            });
        }

        public static void AddAction(this ISequenceable This, Action<IDisposable> action)
        {
            This.Add(() => CompleteOnDispose.Create(x => action(x)));
        }

        public static IDisposable AddWaitForDispose(this ISequenceable This)
        {
            var complete = new CompleteOnDispose();
            This.Add(complete);
            return complete;
        }

        public static void AddDelay(this ISequenceable This, float seconds)
        {
            This.AddDelay(TimeSpan.FromSeconds(seconds));
        }

        public static void AddDelay(this ISequenceable This, float seconds, IScheduler scheduler)
        {
            This.AddDelay(TimeSpan.FromSeconds(seconds), scheduler);
        }

        public static void AddDelay(this ISequenceable This, TimeSpan delay)
        {
            This.AddDelay(delay, Scheduler.DefaultSchedulers.TimeBasedOperations);
        }

        public static void AddDelay(this ISequenceable This, TimeSpan delay, IScheduler scheduler)
        {
            This.Add(Observable.ReturnUnit().Delay(delay, scheduler));
        }
    }
}