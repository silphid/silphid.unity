using System;
using UniRx;

namespace Silphid.Machina
{
	public interface IMachine<TState> where TState : IState
	{
		IReactiveProperty<TState> State { get; }
		IObservable<Change<TState>> Changes { get; }
	}
}