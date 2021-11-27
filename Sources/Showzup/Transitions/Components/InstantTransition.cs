namespace Silphid.Showzup.Components
{
    public class InstantTransition : Transition
    {
        protected override ITransition CreateTransition() =>
            Showzup.InstantTransition.Instance;
    }
}