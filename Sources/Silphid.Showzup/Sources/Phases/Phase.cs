using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class Phase
    {
        public PhaseId Id { get; }
        public Presentation Presentation { get; }
        public Parallel Parallel { get; }

        public Phase(PhaseId id, Presentation presentation)
        {
            Id = id;
            Presentation = presentation;
            Parallel = Parallel.Create();
        }
    }
}