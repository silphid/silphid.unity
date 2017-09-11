using Silphid.Requests;
using UniRx;

namespace Silphid.Machina
{
	public interface IMachine : IRequestHandler
	{
		
	}
	
	public interface IMachine<TState> : IMachine
	{
		ReadOnlyReactiveProperty<TState> State { get; }
	}
}