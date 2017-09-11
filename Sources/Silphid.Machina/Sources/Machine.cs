using System;
using System.Collections.Generic;
using Silphid.Extensions;
using Silphid.Requests;
using UniRx;

namespace Silphid.Machina
{
    public class Machine<TState> : IMachine<TState>
    {
        private readonly Dictionary<TState, StateInfo> _stateInfos = new Dictionary<TState, StateInfo>();
        
        private readonly ReactiveProperty<object> _stateOrMachine;
        public ReadOnlyReactiveProperty<object> StateOrMachine { get; }
        public ReadOnlyReactiveProperty<TState> State { get; }
        public ReadOnlyReactiveProperty<IMachine> SubMachine { get; }

        public Machine(TState initialState = default(TState))
        {
            _stateOrMachine = new ReactiveProperty<object>(initialState);
            StateOrMachine = _stateOrMachine.ToReadOnlyReactiveProperty();
            State = _stateOrMachine.Select(x => x as TState).ToReadOnlyReactiveProperty();
            State = _stateOrMachine.ToReadOnlyReactiveProperty();
        }

        public Machine(IMachine initialSubMachine = null)
        {
            _stateOrMachine = new ReactiveProperty<object>(initialSubMachine);
            State = _stateOrMachine.ToReadOnlyReactiveProperty();
        }

        public void Set(TState state)
        {
            if (!(State.Value?.Equals(state) ?? false))
                State.Value = state;
        }

        public void Set(IMachine machine)
        {
            if (!(State.Value?.Equals(state) ?? false))
                State.Value = state;
        }

        public bool Is(TState state) =>
            IMachineExtensions.IsStateEquivalent(State.Value, state);

        public IRequest Handle(IRequest request)
        {
            if (State.Value == null)
                return request;
            
            if (State.Value ==)
            
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