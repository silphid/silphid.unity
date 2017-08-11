using Silphid.Extensions.DataTypes;

namespace Silphid.Machina
{
    public abstract class State<T> : ObjectEnum<T>, IState where T : State<T>
    {
        public IState BaseState { get; }

        protected State(State<T> baseState = null)
        {
            BaseState = baseState;
        }

        protected State(int id, State<T> baseState = null) : base(id)
        {
            BaseState = baseState;
        }
    }
}