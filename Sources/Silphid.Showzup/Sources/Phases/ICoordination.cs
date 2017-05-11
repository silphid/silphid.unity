using System;
using Rx = UniRx;

namespace Silphid.Showzup
{
    public interface ICoordination : IDisposable
    {
        IDisposable Coordinate(Rx.IObserver<PhaseEvent> observer);
    }
}