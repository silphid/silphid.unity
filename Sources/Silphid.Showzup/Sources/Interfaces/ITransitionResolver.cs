namespace Silphid.Showzup
{
    public interface ITransitionResolver
    {
        ITransition Resolve(Presentation presentation);
    }
}