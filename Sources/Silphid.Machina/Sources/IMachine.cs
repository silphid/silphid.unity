using UniRx;

namespace Silphid.Machina
{
	public interface IMachine<TState>
	{
		IReactiveProperty<TState> State { get; }
		bool Trigger(object trigger);
	}
}