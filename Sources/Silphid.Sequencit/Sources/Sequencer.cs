using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;
using Rx = UniRx;

namespace Silphid.Sequencit
{
    public class Sequencer : ISequenceable, IDisposable
    {
        private readonly Queue<Rx.IObservable<Unit>> _observables = new Queue<Rx.IObservable<Unit>>();
        private IDisposable _currentExecution = Disposable.Empty;
        private bool _isStarted;
        private bool _isExecuting;

        public static Sequencer Create(Action<Sequencer> action)
        {
            var sequence = new Sequencer();
            action(sequence);
            return sequence;
        }

        public static Sequencer Start(Action<Sequencer> action)
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
            _currentExecution.Dispose();
        }

        public void Add(Rx.IObservable<Unit> observable)
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