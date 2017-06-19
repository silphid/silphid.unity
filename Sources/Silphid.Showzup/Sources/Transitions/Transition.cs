using System;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    [Serializable]
    public abstract class Transition : ITransition
    {
        [SerializeField] public float Duration = 0.4f;

        float ITransition.Duration => Duration;
        
        public abstract
            void Prepare(GameObject sourceContainer, GameObject targetContainer, Direction direction);

        public abstract UniRx.IObservable<Unit> Perform(GameObject sourceContainer, GameObject targetContainer,
            Direction direction, float duration);

        public virtual void Complete(GameObject sourceContainer, GameObject targetContainer)
        {
        }
    }
}