using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Machina
{
    public class Machine<TState> : IMachine<TState>
    {
        private readonly Dictionary<TState, StateInfo> _stateInfos = new Dictionary<TState, StateInfo>();
        
        public IReactiveProperty<TState> State { get; }

        public Machine(TState initialState = default(TState))
        {
            State = new ReactiveProperty<TState>(initialState);
        }

        public void Set(TState state)
        {
            if (!(State.Value?.Equals(state) ?? false))
                State.Value = state;
        }

        public bool Is(TState state) =>
            IMachineExtensions.IsStateEquivalent(State.Value, state);

        public bool Trigger(object trigger)
        {
            if (State.Value == null)
                return false;
            
            var stateInfo = _stateInfos.GetValueOrDefault(State.Value);
            var handler = stateInfo?.Handlers.GetValueOrDefault(trigger.GetType());
            return handler?.Invoke(trigger) ?? false;
        }

        public IStateConfig When(TState state) =>
            _stateInfos.GetOrCreateValue(state, () => new StateInfo());

        public IStateConfig When(TState state, Action<IStateConfig> action)
        {
            var config = When(state);
            action(config);
            return config;
        }
    }
}