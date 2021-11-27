using System;

namespace Silphid.Extensions
{
    public class FloatVelocityObservable : VelocityObservableBase<float>
    {
        public FloatVelocityObservable(IObservable<float> source,
                                       float smoothness,
                                       IObservable<float> velocities,
                                       Func<float> getTime)
            : base(source, smoothness, velocities, getTime) {}

        protected override float GetVelocity(float previousValue, float value, float elapsed) =>
            (value - previousValue) / elapsed;

        protected override float GetSmoothed(float previousValue, float value, float smoothness) =>
            value.Smooth(previousValue, smoothness);
    }
}