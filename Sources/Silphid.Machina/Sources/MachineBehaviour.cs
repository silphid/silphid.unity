using UniRx;
using UnityEngine;

namespace Silphid.Machina
{
    public abstract class MachineBehaviour<TState> : MonoBehaviour, IMachine<TState> where TState : class, IState
    {
        private readonly Machine<TState> _machine;

        public IReactiveProperty<TState> State => _machine.State;

        protected MachineBehaviour()
        {
            _machine = new Machine<TState>();
        }

        protected MachineBehaviour(TState initialState)
        {
            _machine = new Machine<TState>(initialState);
        }

        public void Set(TState state) => _machine.State.Value = state;
        public bool Is(TState state) => _machine.State.Value.Is(state);
        public bool Trigger(object trigger) => _machine.Trigger(trigger);
    }
}