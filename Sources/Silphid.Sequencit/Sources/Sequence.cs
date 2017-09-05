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

        public static Sequence Create(params Func<IObservable<Unit>>[] selectors) =>
            Create(seq => selectors.ForEach(selector => seq.Add(Observable.Defer(selector))));

        public static Sequence Create(IEnumerable<IObservable<Unit>> observables) =>
            Create(seq => observables.ForEach(x => seq.Add(x)));

        public static IDisposable Start(Action<Sequence> action) =>
            Create(action).AutoDetach().Subscribe();

        public static IDisposable Start(params Func<IObservable<Unit>>[] selectors) =>
            Start(seq => selectors.ForEach(selector => seq.Add(selector())));

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