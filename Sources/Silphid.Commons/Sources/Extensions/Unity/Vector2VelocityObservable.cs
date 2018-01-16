using System;
using UnityEngine;

namespace Silphid.Extensions
{
    public class Vector2VelocityObservable : VelocityObservableBase<Vector2>
    {
        public Vector2VelocityObservable(IObservable<Vector2> source, float smoothness, IObservable<Vector2> velocities, Func<float> getTime)
            : base(source, smoothness, velocities, getTime)
        {
        }

        protected override Vector2 GetVelocity(Vector2 previousValue, Vector2 value, float elapsed) =>
            (value - previousValue) / elapsed;

        protected override Vector2 GetSmoothed(Vector2 previousValue, Vector2 value, float smoothness) =>
            value.Smooth(previousValue, smoothness);
    }
}