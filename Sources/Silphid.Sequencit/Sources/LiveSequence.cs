using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UniRx;
using Rx = UniRx;

namespace Silphid.Sequencit
{
    public class LiveSequence : ISequencer, IDisposable
    {
        private Queue<IObservable<Unit>> _observables = new Queue<IObservable<Unit>>();
        private IDisposable _currentExecution;
        private bool _isStarted;
        private bool _isExecuting;

        public static LiveSequence Create(Action<LiveSequence> action)
        {
            var sequence = new LiveSequence();
            action(sequence);
            return sequence;
        }

        public static LiveSequence Start(Action<LiveSequence> action)
        {
            var sequence = Create(action);
            sequence.Start();
            return sequence;
        }

        public void Start()
        {
            if (_isStarted)
                return;

            _isStarted = true;
            StartNext();
        }

        public void Stop()
        {
            if (!_isStarted)
                return;

            _isStarted = false;
            _currentExecution?.Dispose();
        }

        public void Clear()
        {
            if (!_isStarted)
                return;

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

        private bool Truncate(IObservable<Unit> observable, bool isInclusive)
        {
            var index = _observables.IndexOf(observable);
            if (index == -1)
                return false;
            
            _observables = new Queue<IObservable<Unit>>(_observables.Take(index));
            return true;
        }

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

        private bool Skip(IObservable<Unit> observable, bool isInclusive)
        {
            if (!_observables.Contains(observable))
                return false;

            if (!_isStarted)
            {
                _isExecuting = false;
                _currentExecution?.Dispose();
            }
            
            while (_observables.Peek() != observable)
                _observables.Dequeue();
            
            if (isInclusive)
                _observables.Dequeue();

            if (_isStarted)
                StartNext();

            return true;
        }

        public void Add(IObservable<Unit> observable)
        {
            _observables.Enqueue(observable);
            StartNext();
        }

        private void StartNext()
        {
            if (!_isStarted || _isExecuting || _observables.Count == 0)
                return;

            var observable = _observables.Dequeue();
            _isExecuting = true;
            _currentExecution = observable
                .DoOnCancel(() => _isExecuting = false)
                .SubscribeCompletion(() =>
            {
                _isExecuting = false;
                StartNext();
            });
        }

        public void Dispose()
        {
            Stop();
        }
    }
}