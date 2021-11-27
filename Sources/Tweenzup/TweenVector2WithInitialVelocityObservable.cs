using System;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Tweenzup
{
    public class TweenVector2WithInitialVelocityObservable : TweenWithInitialVelocityObservableBase<Vector2>
    {
        public TweenVector2WithInitialVelocityObservable(Func<Vector2> sourceSelector,
                                                         Func<Vector2> velocitySelector,
                                                         Vector2 target,
                                                         float duration,
                                                         IEaser easer)
            : base(sourceSelector, velocitySelector, target, duration, easer) {}

        protected override Vector2 Lerp(float ratio, Vector2 source, Vector2 target) =>
            ratio.Lerp(source, target);

        protected override Vector2 Project(Vector2 source, Vector2 velocity, float duration) =>
            source + velocity * duration;
    }
}