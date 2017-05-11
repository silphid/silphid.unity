using UniRx;

namespace Silphid.Showzup
{
    public interface ICoordinator
    {
        IObservable<PhaseEvent> Coordinate(Presentation presentation);
    }
}