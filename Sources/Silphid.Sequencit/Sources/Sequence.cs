using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;
using Rx = UniRx;

namespace Silphid.Sequencit
{
    public class Sequence : ISequenceable, Rx.IObservable<Unit>
    {
        private Rx.IObservable<Unit> _observable = Observable.ReturnUnit();

        public static Sequence Create(Action<Sequence> action)
        {
            var sequence = new Sequence();
            action(sequence);
            return sequence;
        }

        public static Sequence Create(params Func<Rx.IObservable<Unit>>[] selectors) =>
            Create(seq => selectors.ForEach(seq.Add));

        public static Sequence Create(IEnumerable<Rx.IObservable<Unit>> observables) =>
            Create(seq => observables.ForEach(seq.Add));

        public static IDisposable Start(Action<Sequence> action) =>
            Create(action).AutoDetach().Subscribe();

        public static IDisposable Start(params Func<Rx.IObservable<Unit>>[] selectors) =>
            Start(seq => selectors.ForEach(seq.Add));

        public void Add(Rx.IObservable<Unit> observable)
        {
            _observable = _observable.Then(observable);
        }

        public IDisposable Subscribe(Rx.IObserver<Unit> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}