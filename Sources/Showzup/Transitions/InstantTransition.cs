using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class InstantTransition : Transition
    {
        public static readonly ITransition Instance = new InstantTransition();

        public override void Prepare(GameObject sourceContainer, GameObject targetContainer, IOptions options) {}

        public override ICompletable Perform(GameObject sourceContainer,
                                             GameObject targetContainer,
                                             IOptions opitons,
                                             float duration) => Completable.Empty();
    }
}