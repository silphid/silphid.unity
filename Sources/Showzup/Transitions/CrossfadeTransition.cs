using Silphid.Sequencit;
using Silphid.Tweenzup;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class CrossfadeTransition : Transition
    {
        public Ease Ease { protected get; set; } = Ease.InOutCubic;
        protected IEaser Easer => Ease.ToEaser();
        public bool FadeOutSource { get; set; } = true;
        public bool FadeInTarget { get; set; } = true;
        public bool SourceAboveTarget { get; set; } = true;
        public bool IsSequential { get; set; }

        public override void Prepare(GameObject sourceContainer, GameObject targetContainer, IOptions options)
        {
            targetContainer.GetComponent<CanvasGroup>()
                           .alpha = FadeInTarget
                                        ? 0
                                        : 1;

            if (sourceContainer == null)
                return;

            if (SourceAboveTarget)
                sourceContainer.transform.SetAsLastSibling();
            else
                sourceContainer.transform.SetAsFirstSibling();
        }

        public override ICompletable Perform(GameObject sourceContainer,
                                             GameObject targetContainer,
                                             IOptions options,
                                             float duration)
        {
            var sequencer = IsSequential
                                ? (ISequencer) Sequence.Create()
                                : Parallel.Create();
            PerformTransition(
                sourceContainer,
                targetContainer,
                IsSequential
                    ? duration * 0.5f
                    : duration,
                sequencer);
            return sequencer;
        }

        private void PerformTransition(GameObject sourceContainer,
                                       GameObject targetContainer,
                                       float duration,
                                       ISequencer sequencer)
        {
            if (sourceContainer != null && FadeOutSource)
            {
                var canvasGroup = sourceContainer.GetComponent<CanvasGroup>();
                canvasGroup.FadeOut(duration, Easer)
                           .In(sequencer);
            }

            if (FadeInTarget)
            {
                var canvasGroup = targetContainer.GetComponent<CanvasGroup>();
                canvasGroup.FadeIn(duration, Easer)
                           .In(sequencer);
            }
        }

        public override void Complete(GameObject sourceContainer, GameObject targetContainer)
        {
            base.Complete(sourceContainer, targetContainer);

            if (sourceContainer != null)
                sourceContainer.GetComponent<CanvasGroup>()
                               .alpha = 1;

            targetContainer.GetComponent<CanvasGroup>()
                           .alpha = 1;
        }
    }
}