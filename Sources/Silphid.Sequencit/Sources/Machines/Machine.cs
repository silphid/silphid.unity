using Silphid.Extensions;
using UniRx;

namespace Silphid.Sequencit.Machines
{
    public class Machine<TState> : IMachine<TState> where TState : class, IState
    {
        public IReactiveProperty<TState> State { get; private set; }

        public IObservable<Change<TState>> Changes =>
            State.PairWithPrevious().Select(x => new Change<TState>(x.Item1, x.Item2));

        public Machine(TState initialState = null)
        {
            State = new ReactiveProperty<TState>(initialState);
        }

        public void Set(TState state) => State.Value = state;
        public bool Is(TState state) => State.Value.Is(state);
    }
}