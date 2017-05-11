namespace Silphid.Showzup
{
    public abstract class PhaseEvent
    {
        public Phase Phase { get; }

        protected PhaseEvent(Phase phase)
        {
            Phase = phase;
        }
    }
}