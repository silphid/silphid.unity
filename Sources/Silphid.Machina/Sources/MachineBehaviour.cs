using System;
using UniRx;
using UnityEngine;

namespace Silphid.Machina
{
    public abstract class MachineBehaviour<TState> : MonoBehaviour, IMachine<TState> where TState : class, IState
    {
        private readonly Machine<TState> _machine;

        public IReactiveProperty<TState> State => _machine.State;
        public IObservable<Change<TState>> Changes => _machine.Changes;

        protected MachineBehaviour()
        {
            _machine = new Machine<TState>();
        }

        protected MachineBehaviour(TState initialState)
        {
            _machine = new Machine<TState>(initialState);
        }

        public void Set(TState state) => State.Value = state;
        public bool Is(TState state) => State.Value.Is(state);
    }
}