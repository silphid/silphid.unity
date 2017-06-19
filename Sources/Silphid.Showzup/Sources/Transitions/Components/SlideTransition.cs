using UnityEngine;

namespace Silphid.Showzup.Components
{
    public class SlideTransition : CrossfadeTransition
    {
        public Vector2 Offset = Vector2.right;

        protected override ITransition CreateTransition() =>
            new Showzup.SlideTransition
            {
                Ease = Ease,
                FadeOutSource = FadeOutSource,
                FadeInTarget = FadeInTarget,
                SourceAboveTarget = SourceAboveTarget,
                Offset = Offset
            };
    }
}