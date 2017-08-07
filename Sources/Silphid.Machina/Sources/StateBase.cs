using Silphid.Extensions.DataTypes;

namespace Silphid.Machina
{
    public abstract class StateBase<T> : ObjectEnum<T>, IState where T : StateBase<T>
    {
        public IState BaseState { get; }

        protected StateBase(StateBase<T> baseState = null)
        {
            BaseState = baseState;
        }

        protected StateBase(int id, StateBase<T> baseState = null) : base(id)
        {
            BaseState = baseState;
        }
    }
}