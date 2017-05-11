using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UniRx;
using Rx = UniRx;

namespace Silphid.Showzup
{
    public abstract class CoordinationBase : ICoordination
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly List<PhasePerformer> _performers = new List<PhasePerformer>();
        private bool _used;

        protected readonly Presentation Presentation;
        protected Rx.IObserver<PhaseEvent> Observer { get; private set; }

        protected CoordinationBase(Presentation presentation)
        {
            Presentation = presentation;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        protected PhasePerformer CreatePerformer(PhaseId id)
        {
            var performer = new PhasePerformer(new Phase(PhaseId.Present, Presentation), Observer);
            _performers.Add(performer);
            _disposables.Add(performer);
            return performer;
        }

        public IDisposable Coordinate(Rx.IObserver<PhaseEvent> observer)
        {
            if (_used)
                throw new InvalidOperationException("Coordination can be used only once.");
            _used = true;

            Observer = observer;
            _disposables.Add(CoordinateInternal());
            return _disposables;
        }

        protected abstract IDisposable CoordinateInternal();

        protected Rx.IObservable<Unit> CancellationPoint()
        {
            if (Presentation.CancellationToken.IsCancellationRequested)
            {
                _performers
                    .Where(x => x.State == PhaseState.Started)
                    .Reverse()
                    .ForEach(x => x.Cancel());

                return Observable.Throw<Unit>(new PhaseCancelledException());
            }

            return Observable.Empty<Unit>();
        }
    }
}