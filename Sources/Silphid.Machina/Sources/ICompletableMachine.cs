using System;
using UniRx;

namespace Silphid.Machina
{
    public interface ICompletableMachine<TState> : IMachine<TState>, IObservable<Unit>
    {
        IObservable<Unit> Started { get; }
        IObservable<Unit> Completed { get; }
    }
}