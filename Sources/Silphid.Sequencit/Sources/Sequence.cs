using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Sequencit
{
    public class Sequence : SequenceOrParallelBase
    {
        #region Static methods

        public static Sequence Create(Action<ISequencer> action = null) =>
            new Sequence(action);

        public static Sequence Create(params Func<ICompletable>[] selectors) =>
            Create(seq => selectors.ForEach(selector => seq.Add(selector())));

        public static Sequence Create(IEnumerable<ICompletable> observables) =>
            Create(seq => observables.ForEach(x => seq.Add(x)));

        public static Sequence Create(params ICompletable[] observables) =>
            Create(seq => observables.ForEach(x => seq.Add(x)));

        public static IDisposable Start(Action<ISequencer> action) =>
            Create(action).AutoDetach().Subscribe();

        public static IDisposable Start(params ICompletable[] observables) =>
            Create(observables).AutoDetach().Subscribe();

        #endregion

        #region Constructors

        private Sequence(Action<ISequencer> action = null) : base(action)
        {
        }
        
        #endregion
        
        #region ICompletable members

        public override IDisposable Subscribe(ICompletableObserver observer) =>
            GetCompletables()
                .Concat()
                .Subscribe(observer);

        #endregion
    }
}