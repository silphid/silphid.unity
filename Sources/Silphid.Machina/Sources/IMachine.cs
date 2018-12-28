using Silphid.Extensions;
using Silphid.Requests;
using UniRx;

namespace Silphid.Machina
{
	public interface IMachine : IRequestHandler, IDisposer
	{
		IReadOnlyReactiveProperty<object> State { get; }

		void Start(object initialState = null);
		void Complete();
		void SetState(object state);
	}
}