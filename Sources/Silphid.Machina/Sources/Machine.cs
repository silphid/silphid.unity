using System.Collections.Generic;
using Silphid.Extensions;
using Silphid.Machina.Internals;
using UniRx;

namespace Silphid.Machina
{
    public class Machine<TState> : IMachine<TState> where TState : class, IState
    {
        private readonly Dictionary<TState, StateInfo> _stateInfos = new Dictionary<TState, StateInfo>();
        
        public IReactiveProperty<TState> State { get; }

        public Machine(TState initialState = null)
        {
            State = new ReactiveProperty<TState>(initialState);
        }

        public void Set(TState state) =>
            State.Value = state;
        
        public bool Is(TState state) =>
            State.Value.Is(state);

        public bool Trigger(object trigger)
        {
            var stateInfo = _stateInfos.GetValueOrDefault(State.Value);
            var handler = stateInfo?.Handlers.GetValueOrDefault(trigger.GetType());
            return handler?.Invoke(trigger) ?? false;
        }

        public IStateConfig For(TState state) =>
            _stateInfos.GetOrCreateValue(state, () => new StateInfo());
    }
}