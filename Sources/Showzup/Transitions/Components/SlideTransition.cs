using UnityEngine;

namespace Silphid.Showzup.Components
{
    public class SlideTransition : CrossfadeTransition
    {
        public Vector2 Offset = Vector2.right;

        protected override ITransition CreateTransition() =>
            new Showzup.SlideTransition
            {
                Duration = Duration,
                Ease = Ease,
                FadeOutSource = FadeOutSource,
                FadeInTarget = FadeInTarget,
                SourceAboveTarget = SourceAboveTarget,
                Offset = Offset
            };
    }
}