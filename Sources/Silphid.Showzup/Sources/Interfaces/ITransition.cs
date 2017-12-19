using UnityEngine;
using UniRx;

namespace Silphid.Showzup
{
    public interface ITransition
    {
        float Duration { get; }
        
        void Prepare(GameObject sourceContainer, GameObject targetContainer,
            Direction direction);
        
        ICompletable Perform(GameObject sourceContainer, GameObject targetContainer,
            Direction direction, float duration);

        void Complete(GameObject sourceContainer, GameObject targetContainer);
    }
}