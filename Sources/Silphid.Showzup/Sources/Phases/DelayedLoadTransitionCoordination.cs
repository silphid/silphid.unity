using System;
using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class DelayedLoadTransitionCoordination : CoordinationBase
    {
        public DelayedLoadTransitionCoordination(Presentation presentation) : base(presentation)
        {
        }

        protected override IDisposable CoordinateInternal()
        {
            var present = CreatePerformer(PhaseId.Present);
            var show = CreatePerformer(PhaseId.Show);
            var hide = CreatePerformer(PhaseId.Hide);
            var construct = CreatePerformer(PhaseId.Construct);
            var deconstruct = CreatePerformer(PhaseId.Deconstruct);
            var load = CreatePerformer(PhaseId.Load);
            var transition = CreatePerformer(PhaseId.Transition);

            return Sequence.Start(seq =>
            {
                seq.AddAction(() =>
                {
                    present.Start();
                    hide.Start();
                });
                seq.Add(() => deconstruct.Perform());
                seq.Add(CancellationPoint);
                seq.AddAction(() => show.Start());
                seq.Add(() => load.Perform());
                seq.Add(CancellationPoint);
                seq.Add(() => transition.Perform());
                seq.AddAction(() => hide.Complete());
                seq.Add(CancellationPoint);
                seq.Add(() => construct.Perform());
                seq.AddAction(() =>
                {
                    show.Complete();
                    present.Complete();
                    Observer.OnCompleted();
                });
            });
        }
    }
}