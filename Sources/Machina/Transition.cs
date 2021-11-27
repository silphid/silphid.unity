namespace Silphid.Machina
{
    public class Transition
    {
        public object Source { get; }
        public object Target { get; }

        public Transition(object source, object target)
        {
            Source = source;
            Target = target;
        }
    }

    public class Transition<TSource, TTarget>
    {
        public TSource Source { get; }
        public TTarget Target { get; }

        public Transition(TSource source, TTarget target)
        {
            Source = source;
            Target = target;
        }
    }
}