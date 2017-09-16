using UniRx;
using UnityEngine;

namespace Silphid.Machina
{
    public abstract class MachineBehaviour<TState> : MonoBehaviour, IMachine<TState> where TState : class, IState
    {
        private readonly Machine _machine;

        public IReactiveProperty<TState> State => _machine.State;

        protected MachineBehaviour()
        {
            _machine = new Machine();
        }

        protected MachineBehaviour(TState initialState)
        {
            _machine = new Machine(initialState);
        }

        public void Set(TState state) => _machine.State.Value = state;
        public bool Is(TState state) => _machine.State.Value.Is(state);
        public bool Trigger(object trigger) => _machine.Trigger(trigger);
    }
}