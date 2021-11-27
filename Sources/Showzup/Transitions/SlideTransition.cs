using Silphid.Extensions;
using Silphid.Sequencit;
using Silphid.Tweenzup;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class SlideTransition : CrossfadeTransition
    {
        public static readonly ITransition Left = new SlideTransition { Offset = Vector2.right };
        public static readonly ITransition Right = new SlideTransition { Offset = Vector2.left };
        public static readonly ITransition Up = new SlideTransition { Offset = Vector2.down };
        public static readonly ITransition Down = new SlideTransition { Offset = Vector2.up };
        
        public Vector2 Offset { get; set; } = Vector2.right;

        public override void Prepare(GameObject sourceContainer, GameObject targetContainer, IOptions options)
        {
            base.Prepare(sourceContainer, targetContainer, options);

            if (sourceContainer != null)
                ((RectTransform) sourceContainer.transform).anchoredPosition = Vector2.zero;

            var offset = Offset.Multiply(((RectTransform) targetContainer.transform).rect.size) *
                         (options.GetDirectionOrDefault() == Direction.Forward
                              ? 1
                              : -1);
            ((RectTransform) targetContainer.transform).anchoredPosition = offset;
        }

        public override ICompletable Perform(GameObject sourceContainer,
                                             GameObject targetContainer,
                                             IOptions options,
                                             float duration)
        {
            return Parallel.Create(
                parallel =>
                {
                    base.Perform(sourceContainer, targetContainer, options, duration)
                        .In(parallel);

                    var targetTransform = (RectTransform) targetContainer.transform;

                    if (sourceContainer != null)
                    {
                        var sourceTransform = (RectTransform) sourceContainer.transform;
                        var offset = Offset.Multiply(targetTransform.rect.size) *
                                     (options.GetDirectionOrDefault() == Direction.Forward
                                          ? -1
                                          : 1);
                        sourceTransform.MoveAnchorTo(offset, duration, Easer)
                                       .In(parallel);
                    }

                    targetTransform.MoveAnchorTo(Vector2.zero, duration, Easer)
                                   .In(parallel);
                });
        }

        public override void Complete(GameObject sourceContainer, GameObject targetContainer)
        {
            base.Complete(sourceContainer, targetContainer);

            if (sourceContainer != null)
                ((RectTransform) sourceContainer.transform).anchoredPosition = Vector2.zero;

            ((RectTransform) targetContainer.transform).anchoredPosition = Vector2.zero;
        }
    }
}