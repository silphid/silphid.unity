using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Sequencit
{
    public class Parallel : ISequencer
    {
        #region Static methods

        public static Parallel Create(Action<Parallel> action)
        {
            var parallel = new Parallel();
            action(parallel);
            return parallel;
        }

        public static Parallel Create<T>(params Func<IObservable<T>>[] selectors) =>
            Create(p => selectors.ForEach(selector => p.Add(selector())));

        public static Parallel Create<T>(IEnumerable<IObservable<T>> observables) =>
            Create(seq => observables.ForEach(x => seq.Add(x)));

        public static Parallel Create<T>(params IObservable<T>[] observables) =>
            Create(seq => observables.ForEach(x => seq.Add(x)));

        public static IDisposable Start(Action<Parallel> action) =>
            Create(action).AutoDetach().Subscribe();

        public static IDisposable Start<T>(params IObservable<T>[] observables) =>
            Start(p => observables.ForEach(x => x.In(p)));

        #endregion

        #region Private fields

        private List<IObservable<Unit>> _observables;

        #endregion

        #region ISequencer members

        public IObservable<Unit> Add(IObservable<Unit> observable)
        {
            if (_observables == null)
                _observables = new List<IObservable<Unit>>();

            _observables.Add(observable);
            return observable;
        }

        #endregion

        #region IObservable<Unit> members

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

        #endregion
    }
}