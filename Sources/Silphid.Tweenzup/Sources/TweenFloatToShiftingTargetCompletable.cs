using System;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Tweenzup
{
    public class TweenFloatToShiftingTargetCompletable : TweenToShiftingTargetCompletableBase<float>
    {
        public TweenFloatToShiftingTargetCompletable(IReactiveProperty<float> property, IObservable<float> target, float duration, IEaser easer = null) :
            base(property, target, duration, easer)
        {
        }

        protected override IObservable<float> GetVelocity(IReactiveProperty<float> property) =>
            property.Velocity();

        protected override ICompletable Tween(IReactiveProperty<float> property, float target, float duration, float velocity, IEaser easer) =>
            property.TweenTo(target, duration, easer);
    }
}