using System;
using UniRx;
using UniRx.Operators;
using UnityEngine;

namespace Silphid.Extensions
{
    public abstract class VelocityObservableBase<T> : OperatorObservableBase<T>
    {
        private readonly IObservable<T> _source;
        private readonly float _smoothness;
        private readonly IObservable<T> _velocities;
        private readonly Func<float> _getTime;

        protected VelocityObservableBase(IObservable<T> source,
                                         float smoothness,
                                         IObservable<T> velocities,
                                         Func<float> getTime)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            _source = source;
            _smoothness = smoothness;
            _velocities = velocities;
            _getTime = getTime;
        }

        protected abstract T GetVelocity(T previousValue, T value, float elapsed);
        protected abstract T GetSmoothed(T previousValue, T value, float smoothness);

        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel) =>
            new VelocityObserver(
                _source,
                _velocities,
                _getTime,
                GetVelocity,
                GetSmoothed,
                _smoothness,
                observer,
                cancel).Run();

        private class VelocityObserver : OperatorObserverBase<T, T>
        {
            private readonly IObservable<T> _source;
            private readonly IObservable<T> _velocities;
            private readonly Func<float> _getTime;
            private readonly Func<T, T, float, T> _getVelocity;
            private readonly Func<T, T, float, T> _getSmoothed;
            private readonly float _smoothness;
            private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
            private T _lastValue;
            private T _velocity;
            private float? _lastTime;

            public VelocityObserver(IObservable<T> source,
                                    IObservable<T> velocities,
                                    Func<float> getTime,
                                    Func<T, T, float, T> getVelocity,
                                    Func<T, T, float, T> getSmoothed,
                                    float smoothness,
                                    IObserver<T> observer,
                                    IDisposable cancel)
                : base(observer, cancel)
            {
                _source = source;
                _velocities = velocities;
                _getTime = getTime ?? (() => Time.time);
                _getVelocity = getVelocity;
                _getSmoothed = getSmoothed;
                _smoothness = smoothness;
            }

            public IDisposable Run()
            {
                _compositeDisposable.Add(_source.Subscribe(this));

                if (_velocities != null)
                    _compositeDisposable.Add(_velocities.Subscribe(OnNextVelocity, OnError, OnCompleted));

                return _compositeDisposable;
            }

            private void OnNextVelocity(T velocity)
            {
                _velocity = velocity;
                _lastTime = Time.time;
                observer.OnNext(velocity);
            }

            public override void OnNext(T value)
            {
                var time = _getTime();

                if (_lastTime.HasValue)
                {
                    var elapsed = time - _lastTime.Value;
                    if (elapsed >= 0.00001f)
                    {
                        var instantVelocity = _getVelocity(_lastValue, value, elapsed);
                        _velocity = _getSmoothed(_velocity, instantVelocity, _smoothness);
                        observer.OnNext(_velocity);
                    }
                }

                _lastValue = value;
                _lastTime = time;
            }

            public override void OnError(Exception error)
            {
                try
                {
                    observer.OnError(error);
                }
                finally
                {
                    Dispose();
                }
            }

            public override void OnCompleted()
            {
                try
                {
                    observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }

            private new void Dispose()
            {
                _compositeDisposable.Dispose();
                base.Dispose();
            }
        }
    }
}