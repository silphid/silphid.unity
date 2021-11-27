using System;
using UnityEngine;

namespace Silphid.Extensions
{
    public class Vector3VelocityObservable : VelocityObservableBase<Vector3>
    {
        public Vector3VelocityObservable(IObservable<Vector3> source,
                                         float smoothness,
                                         IObservable<Vector3> velocities,
                                         Func<float> getTime)
            : base(source, smoothness, velocities, getTime) {}

        protected override Vector3 GetVelocity(Vector3 previousValue, Vector3 value, float elapsed) =>
            (value - previousValue) / elapsed;

        protected override Vector3 GetSmoothed(Vector3 previousValue, Vector3 value, float smoothness) =>
            value.Smooth(previousValue, smoothness);
    }
}