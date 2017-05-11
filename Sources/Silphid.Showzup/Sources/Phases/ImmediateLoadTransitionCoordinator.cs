namespace Silphid.Showzup
{
    public class ImmediateLoadTransitionCoordinator : CoordinatorBase
    {
        protected override ICoordination CreateCoordination(Presentation presentation) =>
            new ImmediateLoadTransitionCoordination(presentation);
    }
}