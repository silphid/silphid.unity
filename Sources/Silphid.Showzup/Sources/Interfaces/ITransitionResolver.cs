namespace Silphid.Showzup
{
    public interface ITransitionResolver
    {
        Transition Resolve(Presentation presentation);
    }
}