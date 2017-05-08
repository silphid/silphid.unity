using UniRx;

namespace Silphid.Sequencit.Machines
{
	public interface IMachine<TState> where TState : IState
	{
		IReactiveProperty<TState> State { get; }
		IObservable<Change<TState>> Changes { get; }
	}
}