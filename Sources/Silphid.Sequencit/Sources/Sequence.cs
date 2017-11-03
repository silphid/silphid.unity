using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Sequencit
{
    public class Sequence : ISequencer
    {
        #region Public methods

        public static Sequence Create(Action<Sequence> action)
        {
            var sequence = new Sequence();
            action(sequence);
            return sequence;
        }

        public static Sequence Create<T>(params Func<IObservable<T>>[] selectors) =>
            Create(p => selectors.ForEach(selector => p.Add(selector())));

        public static Sequence Create<T>(IEnumerable<IObservable<T>> observables) =>
            Create(seq => observables.ForEach(x => seq.Add(x)));

        public static Sequence Create<T>(params IObservable<T>[] observables) =>
            Create(seq => observables.ForEach(x => seq.Add(x)));

        public static IDisposable Start(Action<Sequence> action) =>
            Create(action).AutoDetach().Subscribe();

        public static IDisposable Start<T>(params IObservable<T>[] observables) =>
            Start(p => observables.ForEach(x => x.In(p)));

        #endregion

        #region Private fields

        private readonly List<IObservable<Unit>> _observables = new List<IObservable<Unit>>();

        #endregion

        #region ISequencer members

        public IObservable<Unit> Add(IObservable<Unit> observable)
        {
            _observables.Add(observable);
            return observable;
        }

        #endregion

        #region IObservable<Unit> members

        public IDisposable Subscribe(IObserver<Unit> observer) =>
            _observables.Concat().Subscribe(observer);

        #endregion        
    }
}