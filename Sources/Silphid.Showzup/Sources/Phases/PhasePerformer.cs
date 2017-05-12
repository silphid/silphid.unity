using System;
using UniRx;
using Rx = UniRx;

namespace Silphid.Showzup
{
    public class PhasePerformer : IDisposable
    {
        private readonly Rx.IObserver<PhaseEvent> _observer;
        private readonly Subject<Unit> _completedSubject = new Subject<Unit>();

        public readonly CompositeDisposable _disposables = new CompositeDisposable();
        public PhaseState State { get; private set; }
        public Phase Phase { get; }
        public Rx.IObservable<Unit> Completed => _completedSubject;

        public PhasePerformer(Phase phase, Rx.IObserver<PhaseEvent> observer)
        {
            Phase = phase;
            _observer = observer;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public Rx.IObservable<Unit> Perform()
        {
            Start();
            return Completed.DoOnCompleted(Complete);
        }

        public void Start()
        {
            if (State != PhaseState.Ready)
                throw new InvalidOperationException($"Cannot start phase currently in state: {State}");

            State = PhaseState.Started;
            _observer.OnNext(new PhaseStarting(Phase));
            _disposables.Add(Phase.Parallel.Subscribe(_completedSubject));
        }

        public void Cancel()
        {
            if (State != PhaseState.Started)
                throw new InvalidOperationException($"Cannot cancel phase currently in state: {State}");

            State = PhaseState.Cancelled;
            _observer.OnNext(new PhaseCancelled(Phase));
        }

        public void Complete()
        {
            if (State != PhaseState.Started)
                throw new InvalidOperationException($"Cannot complete phase currently in state: {State}");

            State = PhaseState.Completed;
            _observer.OnNext(new PhaseCompleted(Phase));
        }
    }
}