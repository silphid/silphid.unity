using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;
using Rx = UniRx;

namespace Silphid.Sequencit
{
    public class SequenceQueue : ISequencer, IDisposable
    {
        private readonly Queue<IObservable<Unit>> _observables = new Queue<IObservable<Unit>>();
        private IDisposable _currentExecution;
        private bool _isStarted;
        private bool _isExecuting;

        public static SequenceQueue Create(Action<SequenceQueue> action)
        {
            var sequence = new SequenceQueue();
            action(sequence);
            return sequence;
        }

        public static SequenceQueue Start(Action<SequenceQueue> action)
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

        public void SkipTo(IObservable<Unit> observable)
        {
            if (!_isStarted || !_observables.Contains(observable))
                return;
            
            _isExecuting = false;
            _currentExecution?.Dispose();
            while (_observables.Peek() != observable)
                _observables.Dequeue();

            StartNext();
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