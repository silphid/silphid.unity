using System;
using DG.Tweening;
using Silphid.Extensions;
using Silphid.Sequencit;
using UniRx;
using UnityEngine;
using Sequence = Silphid.Sequencit.Sequence;

namespace Silphid.Showzup
{
    public class CrossfadeTransition : Transition
    {
        public Ease Ease { get; set; } = Ease.InOutCubic;
        public bool FadeOutSource { get; set; } = true;
        public bool FadeInTarget { get; set; } = true;
        public bool SourceAboveTarget { get; set; } = true;
        public bool IsSequential { get; set; }

        public override void Prepare(GameObject sourceContainer, GameObject targetContainer, Direction direction)
        {
            targetContainer.GetComponent<CanvasGroup>().alpha = FadeInTarget ? 0 : 1;

            if (sourceContainer == null)
                return;
            
            if (SourceAboveTarget)
                sourceContainer.transform.SetAsLastSibling();
            else
                sourceContainer.transform.SetAsFirstSibling();
        }

        public override ICompletable Perform(GameObject sourceContainer, GameObject targetContainer, Direction direction, float duration)
        {
            var sequencer = IsSequential ? (ISequencer) Sequence.Create() : Parallel.Create();
            PerformTransition(sourceContainer, targetContainer, IsSequential ? duration *  0.5f : duration, sequencer);
            return sequencer;
        }

        private void PerformTransition(GameObject sourceContainer, GameObject targetContainer, float duration, ISequencer sequencer)
        {
            if (sourceContainer != null && FadeOutSource)
                sourceContainer.GetComponent<CanvasGroup>()
                    .DOFadeOut(duration)
                    .SetEase(Ease)
                    .SetAutoKill()
                    .In(sequencer);

            if (FadeInTarget)
                targetContainer.GetComponent<CanvasGroup>()
                    .DOFadeIn(duration)
                    .SetEase(Ease)
                    .SetAutoKill()
                    .In(sequencer);
        }

        public override void Complete(GameObject sourceContainer, GameObject targetContainer)
        {
            base.Complete(sourceContainer, targetContainer);

            if (sourceContainer != null)
                sourceContainer.GetComponent<CanvasGroup>().alpha = 1;

            targetContainer.GetComponent<CanvasGroup>().alpha = 1;
        }
    }
}