namespace Silphid.Showzup
{
    public class DelayedLoadTransitionCoordinator : CoordinatorBase
    {
        protected override ICoordination CreateCoordination(Presentation presentation) =>
            new DelayedLoadTransitionCoordination(presentation);
    }
}