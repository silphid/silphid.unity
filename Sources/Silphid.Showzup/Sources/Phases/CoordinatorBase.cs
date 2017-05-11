using UniRx;

namespace Silphid.Showzup
{
    public abstract class CoordinatorBase : ICoordinator
    {
        public IObservable<PhaseEvent> Coordinate(Presentation presentation) =>
            Observable.Using(
                () => CreateCoordination(presentation),
                coordination => Observable.Create<PhaseEvent>(coordination.Coordinate));

        protected abstract ICoordination CreateCoordination(Presentation presentation);
    }
}