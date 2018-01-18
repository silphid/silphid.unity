using System;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Tweenzup
{
    public class TweenVector3WithInitialVelocityObservable : TweenWithInitialVelocityObservableBase<Vector3>
    {
        public TweenVector3WithInitialVelocityObservable(Func<Vector3> sourceSelector, Func<Vector3> velocitySelector, Vector3 target, float duration, IEaser easer) :
            base(sourceSelector, velocitySelector, target, duration, easer)
        {
        }

        protected override Vector3 Lerp(float ratio, Vector3 source, Vector3 target) =>
            ratio.Lerp(source, target);

        protected override Vector3 Project(Vector3 source, Vector3 velocity, float duration) =>
            source + velocity * duration;
    }
}