using UniRx;

namespace Silphid.Machina
{
	public interface IMachine<TState> where TState : IState
	{
		IReactiveProperty<TState> State { get; }
		bool Trigger(object trigger);
	}
}