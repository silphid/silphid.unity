using System;
using UniRx;

namespace Silphid.Machina
{
    public interface ICompletableMachine<TState> : IMachine<TState>, IObservable<Unit> where TState : IState
    {
        IObservable<Unit> Started { get; }
        IObservable<Unit> Completed { get; }
    }
}