using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;
using Rx = UniRx;

namespace Silphid.Sequencit
{
    public class Parallel : ISequenceable, Rx.IObservable<Unit>
    {
        public static Parallel Create(Action<Parallel> action)
        {
            var parallel = new Parallel();
            action(parallel);
            return parallel;
        }

        public static Parallel Create(params Func<Rx.IObservable<Unit>>[] selectors) =>
            Create(p => selectors.ForEach(p.Add));

        public static Parallel Create(IEnumerable<Rx.IObservable<Unit>> observables) =>
            Create(seq => observables.ForEach(seq.Add));

        public static IDisposable Start(Action<Parallel> action) =>
            Create(action).AutoDetach().Subscribe();

        public static IDisposable Start(params Rx.IObservable<Unit>[] observables) =>
            Start(p => observables.ForEach(x => x.In(p)));

        private List<Rx.IObservable<Unit>> _observables;

        public void Add(Rx.IObservable<Unit> observable)
        {
            if (_observables == null)
                _observables = new List<Rx.IObservable<Unit>>();

            _observables.Add(observable);
        }

        public IDisposable Subscribe(Rx.IObserver<Unit> observer)
        {
            if (_observables == null)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }

            return _observables
                .WhenAll()
                .Subscribe(observer);
        }
    }
}