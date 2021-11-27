using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class NavPresentation
    {
        public IView Source { get; }
        public IView Target { get; }
        public IOptions Options { get; }
        public ITransition Transition { get; }
        public float Duration { get; }
        public Parallel Parallel { get; }

        public NavPresentation(IView source,
                               IView target,
                               IOptions options,
                               ITransition transition,
                               float duration,
                               Parallel parallel)
        {
            Source = source;
            Target = target;
            Options = options;
            Transition = transition;
            Duration = duration;
            Parallel = parallel;
        }

        public override string ToString() => $"Source: {Source}, Target: {Target}";
    }
}