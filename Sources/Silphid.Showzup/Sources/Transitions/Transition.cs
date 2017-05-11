using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public abstract class Transition : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.4f;

        public float Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public abstract
            void Prepare(GameObject sourceContainer, GameObject targetContainer, Direction direction);

        public abstract IObservable<Unit> Perform(GameObject sourceContainer, GameObject targetContainer,
            Direction direction, float duration);

        public virtual void Complete(GameObject sourceContainer, GameObject targetContainer)
        {
        }
    }
}