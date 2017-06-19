using DG.Tweening;

namespace Silphid.Showzup.Components
{
    public class CrossfadeTransition : Transition
    {
        public Ease Ease = Ease.InOutCubic;
        public bool FadeOutSource = true;
        public bool FadeInTarget = true;
        public bool SourceAboveTarget = true;

        protected override ITransition CreateTransition() =>
            new Showzup.CrossfadeTransition
            {
                Ease = Ease,
                FadeOutSource = FadeOutSource,
                FadeInTarget = FadeInTarget,
                SourceAboveTarget = SourceAboveTarget
            };
    }
}