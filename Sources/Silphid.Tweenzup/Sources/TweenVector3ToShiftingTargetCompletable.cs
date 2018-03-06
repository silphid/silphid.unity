using System;
using Silphid.Extensions;
using UniRx;
using UnityEngine;

namespace Silphid.Tweenzup
{
    public class TweenVector3ToShiftingTargetCompletable : TweenToShiftingTargetCompletableBase<Vector3>
    {
        public TweenVector3ToShiftingTargetCompletable(IReactiveProperty<Vector3> property, IObservable<Vector3> target, float duration, IEaser easer = null) :
            base(property, target, duration, easer)
        {
        }

        protected override IObservable<Vector3> GetVelocity(IReactiveProperty<Vector3> property) =>
            property.Velocity();

        protected override ICompletable Tween(IReactiveProperty<Vector3> property, Vector3 target, float duration, Vector3 velocity, IEaser easer) =>
            property.TweenTo(target, duration, () => velocity, easer);
    }
}