namespace Silphid.Showzup.Components
{
    public class ZoomTransition : CrossfadeTransition
    {
        public float StartScale = 0.8f;
        public float EndScale = 1.2f;

        protected override ITransition CreateTransition() =>
            new Showzup.ZoomTransition
            {
                Duration = Duration,
                Ease = Ease,
                FadeOutSource = FadeOutSource,
                FadeInTarget = FadeInTarget,
                SourceAboveTarget = SourceAboveTarget,
                StartScale = StartScale,
                EndScale = EndScale
            };
    }
}