using System;

namespace Silphid.Showzup
{
    public interface ICoordination : IDisposable
    {
        IDisposable Coordinate(IObserver<PhaseEvent> observer);
    }
}