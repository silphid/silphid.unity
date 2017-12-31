using Silphid.Tweenzup;

namespace Silphid.Showzup.Components
{
    public class CrossfadeTransition : Transition
    {
        public Ease Ease = Ease.InOutCubic;
        public bool FadeOutSource = true;
        public bool FadeInTarget = true;
        public bool SourceAboveTarget = true;
        public bool IsSequential;

        protected override ITransition CreateTransition() =>
            new Showzup.CrossfadeTransition
            {
                Duration = Duration,
                Ease = Ease,
                FadeOutSource = FadeOutSource,
                FadeInTarget = FadeInTarget,
                IsSequential = IsSequential
            };
    }
}