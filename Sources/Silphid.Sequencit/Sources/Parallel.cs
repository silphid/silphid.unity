using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Sequencit
{
    public class Parallel : SequenceOrParallelBase
    {
        #region Static methods

        public static Parallel Create(Action<ISequencer> action = null) =>
            new Parallel(action);

        public static Parallel Create<T>(params Func<IObservable<T>>[] selectors) =>
            Create(seq => selectors.ForEach(selector => seq.Add(selector())));

        public static Parallel Create<T>(IEnumerable<IObservable<T>> observables) =>
            Create(seq => observables.ForEach(x => seq.Add(x)));

        public static Parallel Create<T>(params IObservable<T>[] observables) =>
            Create(seq => observables.ForEach(x => seq.Add(x)));

        public static IDisposable Start(Action<ISequencer> action) =>
            Create(action).AutoDetach().Subscribe();

        public static IDisposable Start<T>(params IObservable<T>[] observables) =>
            Create(observables).AutoDetach().Subscribe();

        #endregion

        #region Constructors

        private Parallel(Action<ISequencer> action = null) : base(action)
        {
        }
        
        #endregion
        
        #region IObservable<Unit> members

        public override IDisposable Subscribe(IObserver<Unit> observer) =>
            GetObservables()
                .WhenAll()
                .Subscribe(observer);

        #endregion
    }
}