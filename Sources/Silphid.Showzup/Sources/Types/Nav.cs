using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class Nav
    {
        public IView Source { get; }
        public IView Target { get; }
        public Parallel Parallel { get; }
        public Transition Transition { get; }
        public float Duration { get; }

        public Nav(IView source, IView target, Parallel parallel, Transition transition, float duration)
        {
            Source = source;
            Target = target;
            Parallel = parallel;
            Transition = transition;
            Duration = duration;
        }

        public override string ToString() => $"Source: {Source}, Target: {Target}";
    }}