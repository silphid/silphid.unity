using System;
using Silphid.Extensions;

namespace Silphid.Tweenzup
{
    public class TweenFloatWithInitialVelocityObservable : TweenWithInitialVelocityObservableBase<float>
    {
        public TweenFloatWithInitialVelocityObservable(Func<float> sourceSelector,
                                                       Func<float> velocitySelector,
                                                       float target,
                                                       float duration,
                                                       IEaser easer)
            : base(sourceSelector, velocitySelector, target, duration, easer) {}

        protected override float Lerp(float ratio, float source, float target) =>
            ratio.Lerp(source, target);

        protected override float Project(float source, float velocity, float duration) =>
            source + velocity * duration;
    }
}