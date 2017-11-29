using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Sequencit
{
    public class LiveSequence : ISequencer, IDisposable
    {
        #region Static methods

        public static LiveSequence Create(Action<LiveSequence> action)
        {
            var sequence = new LiveSequence();
            action(sequence);
            return sequence;
        }

        public static LiveSequence Start()
        {
            var liveSequence = new LiveSequence();
            liveSequence.SubscribeAndForget();
            return liveSequence;
        }

        public static IDisposable Start(Action<LiveSequence> action) =>
            Create(action).Subscribe();

        #endregion

        #region Private fields

        private Queue<IObservable<Unit>> _observables = new Queue<IObservable<Unit>>();
        private IDisposable _currentExecution;
        private bool _isStarted;
        private bool _isExecuting;
        private Lapse _subscriptionLapse;
        
        #endregion
        
        #region IObservable<Unit> members

        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            if (!_isStarted)
            {
                _subscriptionLapse = Lapse.Create(); 
                _isStarted = true;
                StartNext();
            }

            return new CompositeDisposable(
                Disposable.Create(Complete),
                _subscriptionLapse.Subscribe(observer));
        }

        #endregion

        #region ISequencer members

        public IObservable<Unit> Add(IObservable<Unit> observable)
        {
            _observables.Enqueue(observable);
            StartNext();
            return observable;
        }

        #endregion

        #region IDisposable members

        public void Dispose()
        {
            Complete();
        }

        #endregion
        
        #region Public methods

        public void Complete()
        {
            if (!_isStarted)
                return;

            _subscriptionLapse.Dispose();
            _subscriptionLapse = null;
            _isStarted = false;
            _currentExecution?.Dispose();
        }
        
        public void Clear()
        {
            _observables.Clear();
            _isExecuting = false;
            _currentExecution?.Dispose();
        }

        /// <summary>
        /// Removes observables from sequence, starting with given and up to the end.
        /// Currently executing item (if any) is not affected by this operation because
        /// it is now longer in the queue.
        /// </summary>
        /// <returns>Whether the given item was found and the truncation occurred.</returns>
        public bool TruncateBefore(IObservable<Unit> observable) =>
            Truncate(observable, isInclusive: true);

        /// <summary>
        /// Removes observables from sequence, starting after given item and up to the end.
        /// Currently executing item (if any) is not affected by this operation because
        /// it is now longer in the queue.
        /// </summary>
        /// <returns>Whether the given item was found and the truncation occurred.</returns>
        public bool TruncateAfter(IObservable<Unit> observable) =>
            Truncate(observable, isInclusive: false);

        /// <summary>
        /// Removes observables from sequence, starting with first one and up to given item exclusively.
        /// If the sequence was running, currently executing item (if any) is cancelled/disposed and the
        /// next item in queue (if any) is executed.
        /// </summary>
        /// <returns>Whether the given item was found and the truncation occurred.</returns>
        public bool SkipBefore(IObservable<Unit> observable) =>
            Skip(observable, isInclusive: false);

        /// <summary>
        /// Removes observables from sequence, starting with first one and up to given item inclusively.
        /// If the sequence was running, currently executing item (if any) is cancelled/disposed and the
        /// next item in queue (if any) is executed.
        /// </summary>
        /// <returns>Whether the given item was found and the truncation occurred.</returns>
        public bool SkipAfter(IObservable<Unit> observable) =>
            Skip(observable, isInclusive: true);

        #endregion

        #region Private methods

        private bool Skip(IObservable<Unit> observable, bool isInclusive)
        {
            if (!_observables.Contains(observable))
                return false;

            _isExecuting = false;
            _currentExecution?.Dispose();
            
            while (_observables.Peek() != observable)
                _observables.Dequeue();
            
            if (isInclusive)
                _observables.Dequeue();

            if (_isStarted)
                StartNext();

            return true;
        }

        private bool Truncate(IObservable<Unit> observable, bool isInclusive)
        {
            var index = _observables.IndexOf(observable);
            if (index == null)
                return false;

            var count = index.Value + (isInclusive ? 0 : 1);
            _observables = new Queue<IObservable<Unit>>(_observables.Take(count));
            return true;
        }

        private void StartNext()
        {
            if (!_isStarted || _isExecuting || _observables.Count == 0)
                return;

            var observable = _observables.Dequeue();
            _isExecuting = true;
            _currentExecution = observable
                .Finally(() =>
                {
                    _isExecuting = false;
                    StartNext();
                })
                .Subscribe();
        }

        #endregion
    }
}