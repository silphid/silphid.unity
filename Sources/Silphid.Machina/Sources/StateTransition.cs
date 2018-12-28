namespace Silphid.Machina
{
	public class StateTransition
    {
        public object Source { get; }
        public object Target { get; }

        public StateTransition(object source, object target)
        {
            Source = source;
            Target = target;
        }
    }

    public class StateTransition<TSource, TTarget>
    {
        public TSource Source { get; }
        public TTarget Target { get; }

        public StateTransition(TSource source, TTarget target)
        {
            Source = source;
            Target = target;
        }
    }
}