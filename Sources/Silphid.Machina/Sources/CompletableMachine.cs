using System;
using Silphid.Sequencit;
using UniRx;

namespace Silphid.Machina
{
    public class CompletableMachine<TState> : Machine<TState>, ICompletableMachine<TState>
    {
        private readonly Lapse _lapse = new Lapse();
        private readonly Subject<Unit> _startedSubject = new Subject<Unit>();
        private readonly Subject<Unit> _completedSubject = new Subject<Unit>();

        public CompletableMachine(TState initialState = default(TState)) : base(initialState)
        {
        }

        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            OnStarted();
            return _lapse.Subscribe(observer);
        }

        public void Complete()
        {
            OnCompleted();
            _lapse.Dispose();
        }

        public IObservable<Unit> Started => _startedSubject;
        public IObservable<Unit> Completed => _completedSubject;

        protected virtual void OnStarted()
        {
            _startedSubject.OnNext(Unit.Default);
        }

        protected virtual void OnCompleted()
        {
            _completedSubject.OnNext(Unit.Default);
        }
    }
}