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

        public static Parallel Create(params Func<ICompletable>[] selectors) =>
            Create(seq => selectors.ForEach(selector => seq.Add(selector())));

        public static Parallel Create(IEnumerable<ICompletable> observables) =>
            Create(seq => observables.ForEach(x => seq.Add(x)));

        public static Parallel Create(params ICompletable[] observables) =>
            Create(seq => observables.ForEach(x => seq.Add(x)));

        public static IDisposable Start(Action<ISequencer> action) =>
            Create(action)
               .AutoDetach()
               .Subscribe();

        public static IDisposable Start(Action<ISequencer> action, Action<Exception> onError) =>
            Create(action)
               .AutoDetach()
               .Subscribe(onError);

        public static IDisposable Start(params ICompletable[] observables) =>
            Create(observables)
               .AutoDetach()
               .Subscribe();

        #endregion

        #region Constructors

        private Parallel(Action<ISequencer> action = null)
            : base(action) {}

        #endregion

        #region ICompletable members

        public override IDisposable Subscribe(ICompletableObserver observer) =>
            GetCompletables()
               .WhenAll()
               .Subscribe(observer);

        #endregion
    }
}