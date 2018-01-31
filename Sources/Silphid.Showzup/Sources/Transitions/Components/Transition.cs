using UniRx;
using UnityEngine;

namespace Silphid.Showzup.Components
{
    public abstract class Transition : MonoBehaviour, ITransition
    {
        private ITransition _transition;

        protected abstract ITransition CreateTransition();
        protected ITransition GetTransition() => _transition ?? (_transition = CreateTransition());

        public float Duration = 1f;

        float ITransition.Duration =>
            GetTransition().Duration;
        
        public void Prepare(GameObject sourceContainer, GameObject targetContainer, Direction direction) =>
            GetTransition().Prepare(sourceContainer, targetContainer, direction);

        public ICompletable Perform(GameObject sourceContainer, GameObject targetContainer, Direction direction, float duration) =>
            GetTransition().Perform(sourceContainer, targetContainer, direction, duration);

        public void Complete(GameObject sourceContainer, GameObject targetContainer) =>
            GetTransition().Complete(sourceContainer, targetContainer);
    }
}