using System;
using Silphid.Extensions;
using UniRx;
using UnityEngine;

namespace Silphid.Tweenzup
{
    public class TweenVector2ToShiftingTargetCompletable : TweenToShiftingTargetCompletableBase<Vector2>
    {
        public TweenVector2ToShiftingTargetCompletable(IReactiveProperty<Vector2> property,
                                                       IObservable<Vector2> target,
                                                       float duration,
                                                       IEaser easer = null)
            : base(property, target, duration, easer) {}

        protected override IObservable<Vector2> GetVelocity(IReactiveProperty<Vector2> property) =>
            property.Velocity();

        protected override ICompletable Tween(IReactiveProperty<Vector2> property,
                                              Vector2 target,
                                              float duration,
                                              Vector2 velocity,
                                              IEaser easer) =>
            property.TweenTo(target, duration, () => velocity, easer);
    }
}