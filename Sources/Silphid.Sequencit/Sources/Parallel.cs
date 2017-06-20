using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Sequencit
{
    public class Parallel : ISequencer, IObservable<Unit>
    {
        public static Parallel Create(Action<Parallel> action)
        {
            var parallel = new Parallel();
            action(parallel);
            return parallel;
        }

        public static Parallel Create(params Func<IObservable<Unit>>[] selectors) =>
            Create(p => selectors.ForEach(selector => p.Add(selector())));

        public static Parallel Create(IEnumerable<IObservable<Unit>> observables) =>
            Create(seq => observables.ForEach(seq.Add));

        public static IDisposable Start(Action<Parallel> action) =>
            Create(action).AutoDetach().Subscribe();

        public static IDisposable Start(params IObservable<Unit>[] observables) =>
            Start(p => observables.ForEach(x => x.In(p)));

        private List<IObservable<Unit>> _observables;

        public void Add(IObservable<Unit> observable)
        {
            if (_observables == null)
                _observables = new List<IObservable<Unit>>();

            _observables.Add(observable);
        }

        public IDisposable Subscribe(IObserver<Unit> observer)
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