using System;
using UniRx;
using UnityEngine;
using Trans = Silphid.Showzup.Transition;

namespace Silphid.Showzup.Components
{
    public abstract class Transition : MonoBehaviour, ITransition
    {
        private ITransition _transition;

        protected abstract ITransition CreateTransition();
        protected ITransition GetTransition() => _transition ?? (_transition = CreateTransition());

        public float Duration =>
            GetTransition().Duration;
        
        public void Prepare(GameObject sourceContainer, GameObject targetContainer, Direction direction) =>
            GetTransition().Prepare(sourceContainer, targetContainer, direction);

        public IObservable<Unit> Perform(GameObject sourceContainer, GameObject targetContainer, Direction direction, float duration) =>
            GetTransition().Perform(sourceContainer, targetContainer, direction, duration);

        public void Complete(GameObject sourceContainer, GameObject targetContainer) =>
            GetTransition().Complete(sourceContainer, targetContainer);
    }
}