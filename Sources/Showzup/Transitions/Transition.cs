using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public abstract class Transition : ITransition
    {
        public float Duration { get; set; } = 0.4f;

        public abstract void Prepare(GameObject sourceContainer, GameObject targetContainer, IOptions options);

        public abstract ICompletable Perform(GameObject sourceContainer,
                                             GameObject targetContainer,
                                             IOptions options,
                                             float duration);

        public virtual void Complete(GameObject sourceContainer, GameObject targetContainer) {}
    }
}